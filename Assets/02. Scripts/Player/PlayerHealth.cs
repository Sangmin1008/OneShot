using System;
using Fusion;
using UnityEngine;

namespace OneShot
{
    public class PlayerHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField] public float maxHp = 100f;
        [Networked] private float CurrentHp { get; set; }
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                CurrentHp = maxHp;
            }

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotTo, false);
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change == nameof(CurrentHp))
                {
                    Logger.Log($"[PlayerHP] HP 변경됨: {CurrentHp}");
                }
            }
        }

        public void TakeDamage(float damage, PlayerRef shooter)
        {
            if (!HasStateAuthority) return;
            CurrentHp = Mathf.Max(0f, CurrentHp - damage);
            Logger.Log($"[PlayerHP] HP: {CurrentHp}/{maxHp}");

            if (CurrentHp <= 0)
            {
                // TODO 사망 및 리스폰 처리
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

