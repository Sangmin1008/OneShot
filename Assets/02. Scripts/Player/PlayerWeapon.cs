using System;
using Fusion;
using UnityEngine;

namespace OneShot
{
    public class PlayerWeapon : NetworkBehaviour
    {
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireRate = 0.15f;
        [SerializeField] private float range = 150f;
        
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletImpactPrefab;

        [SerializeField] private AudioClip fireSfx;

        [SerializeField] private LayerMask hitMask;

        // Host에서만 _fireCount 증가 -> 브로드캐스팅 -> 모든 클라이언트의 해당 플레이어에게 Render()에서 VFX 재생
        [Networked] private int _fireCount { get; set; }
        [Networked] private TickTimer _fireCooldown { get; set; }
        [Networked] private Vector3 _lastImpactPoint { get; set; }
        [Networked] private Vector3 _lastImpactNormal { get; set; }
        [Networked] private NetworkBool _hasImpact { get; set; }
        
        private ChangeDetector _changeDetector;
        private AudioSource _fireSound;

        private void Start()
        {
            _fireSound = gameObject.AddComponent<AudioSource>();
            _fireSound.loop = false;
            _fireSound.playOnAwake = false;
        }

        #region Fusion 메서드 오버라이드

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotTo, false);
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out PlayerNetworkInput input)) return;

            bool isFire = input.Buttons.IsSet((int)PlayerButton.Fire) && _fireCooldown.ExpiredOrNotRunning(Runner);
            if (isFire)
            {
                // 발사메서드
                Fire(input.AimDirection, input.AimTargetPoint);
            }
        }

        #endregion

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change == nameof(_fireCount))
                {
                    muzzleFlash?.Play();
                    _fireSound?.PlayOneShot(fireSfx);
                }

                if (_hasImpact)
                {
                    var obj = Instantiate(bulletImpactPrefab, _lastImpactPoint, Quaternion.LookRotation(_lastImpactNormal));
                    Destroy(obj, 5f);
                }
            }
        }

        #region 발사 로직 및 히트 판정

        private void Fire(Vector3 direction, Vector3 target)
        {
            // 초를 틱 단위로 변환
            _fireCooldown = TickTimer.CreateFromSeconds(Runner, fireRate);
            
            // 발사 방향 벡터 계산 (총구 -> 조준 대상)
            Vector3 fireDir = (target - firePoint.position).normalized;
            
            // Host에서 발사여부와 히트 판정
            if (HasStateAuthority)
            {
                _fireCount++; // 발사 횟수를 증가 -> 모든 클라이언트의 해당 플레이어에게 동기화 -> Render()에서 VFX 재생
                
                // 레이캐스트 발사하는 로직
                FireCast(firePoint.position, fireDir);
            }
        }
        
        private void FireCast(Vector3 origin, Vector3 direction)
        {
            var hitOptions = HitOptions.IncludePhysX | HitOptions.SubtickAccuracy | HitOptions.IgnoreInputAuthority;
            bool isHit = Runner.LagCompensation.Raycast(
                origin: origin,
                direction: direction,
                length: range,
                player: Object.InputAuthority,
                hit: out LagCompensatedHit hit,
                layerMask: hitMask,
                options: hitOptions);

            _hasImpact = isHit;

            if (isHit)
            {
                _lastImpactPoint = hit.Point;
                _lastImpactNormal = hit.Normal;
            }
            
            if (!isHit) return;
            
            // HitBox 히트 처리 -> 데미지 처리
            if (hit.Hitbox != null)
            {
                Logger.Log($"[PlayerWeapon] 히트: {hit.Hitbox.HitboxIndex} {hit.Hitbox.gameObject.name}");

                if (hit.Hitbox.Root.TryGetComponent<IDamageable>(out var damageable))
                {
                    float dmg = (HitboxType)hit.Hitbox.HitboxIndex switch
                    {
                        HitboxType.Head => damage * 2f,
                        HitboxType.Body => damage,
                        _ => damage * 0.7f,
                    };
                    
                    damageable.TakeDamage(dmg, Object.InputAuthority);
                    Logger.Log($"[PlayerWeapon] 피격 부위: {hit.Hitbox.HitboxIndex} 데미지: {dmg}");
                }
            }
        }
        #endregion

    }
}