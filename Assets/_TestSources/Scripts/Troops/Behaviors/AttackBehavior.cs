using IslandDefense.Troops;
using UnityEngine;

/// <summary>
    /// Implementation of Attack behavior - moves towards and attacks enemy
    /// </summary>
    public class AttackBehavior : SteeringBehavior
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float maxAttackForce = 20f;
        
        private float lastAttackTime = -1000f; // Initialize with a very small value to allow immediate first attack
        private bool isAttacking = false;
        
        public override Vector3 CalculateForce()
        {
            if (troopBase.TargetTroop == null)
                return Vector3.zero;
                
            Vector3 toTarget = troopBase.TargetTroop.transform.position - transform.position;
            float distanceToTarget = toTarget.magnitude;
            
            // If we're within attack range
            if (distanceToTarget <= troopBase.AttackRange)
            {
                // If attack is off cooldown, trigger attack
                if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                {
                    PerformAttack();
                }
                
                // Stop movement when in attack range
                return -troopBase.CurrentVelocity;
            }
            else
            {
                // Get a random approach vector based on probability
                Vector3 approachVector = config.GetRandomDirectionVector();
                
                // Convert the local direction to world space
                approachVector = transform.TransformDirection(approachVector);
                
                // Calculate desired position off to the side of the target
                Vector3 targetPosition = troopBase.TargetTroop.transform.position + 
                                       (approachVector.normalized * troopBase.AttackRange * 0.8f);
                
                // Calculate desired velocity towards that position
                Vector3 desiredVelocity = (targetPosition - transform.position).normalized * troopBase.MoveSpeed;
                
                // Steering = desired velocity - current velocity
                Vector3 steeringForce = desiredVelocity - troopBase.CurrentVelocity;
                
                // Limit steering force
                if (steeringForce.magnitude > maxAttackForce)
                {
                    steeringForce = steeringForce.normalized * maxAttackForce;
                }
                
                return steeringForce;
            }
        }
        
        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            
            // Play attack animation through the view
            TroopView view = GetComponentInParent<TroopView>();
            if (view != null)
            {
                view.PlayAttackAnimation(() => {
                    // Deal damage after animation
                    if (troopBase.TargetTroop != null)
                    {
                        troopBase.TargetTroop.TakeDamage(troopBase.AttackPower);
                    }
                    isAttacking = false;
                });
            }
            else
            {
                // No view, apply damage immediately
                if (troopBase.TargetTroop != null)
                {
                    troopBase.TargetTroop.TakeDamage(troopBase.AttackPower);
                }
                isAttacking = false;
            }
        }
        
        public override void OnDrawGizmos()
        {
            if (troopBase != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, troopBase.AttackRange);
                
                if (troopBase.TargetTroop != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, troopBase.TargetTroop.transform.position);
                }
            }
        }
        
        public override void InitializeFromConfig(BehaviorConfig behaviorConfig)
        {
            base.InitializeFromConfig(behaviorConfig);
            attackCooldown = 1f / troopBase.AttackSpeed;
        }
    }