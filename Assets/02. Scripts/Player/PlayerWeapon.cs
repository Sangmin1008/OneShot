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

        // Host에서만 _fireCount 증가 -> 브로드캐스팅 -> 모든 클라이언트의 해당 플레이어에게 Render()에서 VFX 재생
        [Networked] private int _fireCount { get; set; }
        [Networked] private TickTimer _fireCooldown { get; set; }
        
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
                    muzzleFlash.Play();
                    _fireSound?.PlayOneShot(fireSfx);
                }
            }
        }

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
                
                // TODO 레이캐스트 발사하는 로직
            }
        }
    }
}