using System;
using Fusion;
using UnityEngine;

namespace OneShot
{
    public class PlayerHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField] public float maxHp = 100f;
        public event Action<float, float> OnHpChanged; // HUD 용 이벤트
        public event Action OnDeath; // PlayerSpawner에서 구독 Despawn 처리
        private bool _isDead;
        
        [Networked] private float CurrentHp { get; set; }
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                CurrentHp = maxHp;
                _isDead = false;
            }

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotTo, false);
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change == nameof(CurrentHp))
                {
                    // Logger.Log($"[PlayerHP] HP 변경됨: {CurrentHp}");
                    OnHpChanged?.Invoke(CurrentHp, maxHp);
                }
            }
        }

        public void TakeDamage(float damage, PlayerRef shooter)
        {
            if (!HasStateAuthority) return;
            if (_isDead) return;
            
            CurrentHp = Mathf.Max(0f, CurrentHp - damage);
            Logger.Log($"[PlayerHP] HP: {CurrentHp}/{maxHp}");

            if (CurrentHp <= 0)
            {
                // Runner.IsForward : 재시뮬레이션 중에는 false -> 사망처리 중복 방지용
                if (!Runner.IsForward) return;
                
                _isDead = true;
                
                // 사망 및 리스폰 처리
                OnDeath?.Invoke();
                Logger.Log($"[PlayerHP] 공격자 {shooter}에 의해 사망했습니다.");
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.Lerp(Color.red, Color.green, CurrentHp/maxHp);
            Gizmos.DrawSphere(transform.position + (Vector3.up * 2f), 0.3f);
        }
    }
}

