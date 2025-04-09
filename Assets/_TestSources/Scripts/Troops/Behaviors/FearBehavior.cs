using UnityEngine;

/// <summary>
    /// Implementation of Fear behavior - temporarily makes troop hesitate or flee
    /// </summary>
    public class FearBehavior : SteeringBehavior
    {
        [Header("Fear Settings")]
        [SerializeField] private float fearDuration = 2f;
        [SerializeField] private float maxFearForce = 15f;
        [SerializeField] private float hesitationChance = 0.7f; // Chance to hesitate vs flee
        
        private float fearEndTime = -1000f;
        private bool isHesitating = false;
        private Vector3 fleeDirection;
        
        public override bool ShouldApply()
        {
            // Should apply if already in fear state
            if (Time.time < fearEndTime)
                return true;
                
            // Otherwise use normal probability check
            return base.ShouldApply();
        }
        
        public override Vector3 CalculateForce()
        {
            // If not in fear state yet, start it
            if (Time.time >= fearEndTime && base.ShouldApply())
            {
                StartFearState();
            }
            
            // If fear has expired, return zero force
            if (Time.time >= fearEndTime)
                return Vector3.zero;
                
            // Hesitation - just stop in place
            if (isHesitating)
            {
                return -troopBase.CurrentVelocity; // Counteract current velocity
            }
            // Fleeing - move in the flee direction
            else
            {
                Vector3 desiredVelocity = fleeDirection * troopBase.MoveSpeed;
                Vector3 steeringForce = desiredVelocity - troopBase.CurrentVelocity;
                
                // Limit steering force
                if (steeringForce.magnitude > maxFearForce)
                {
                    steeringForce = steeringForce.normalized * maxFearForce;
                }
                
                return steeringForce;
            }
        }
        
        private void StartFearState()
        {
            fearEndTime = Time.time + fearDuration;
            
            // Decide whether to hesitate or flee
            if (Random.value < hesitationChance)
            {
                isHesitating = true;
                
                // Play hesitation animation
                TroopView view = GetComponentInParent<TroopView>();
                if (view != null)
                {
                    view.PlayFearAnimation(null);
                }
            }
            else
            {
                isHesitating = false;
                
                // Choose a random direction to flee
                fleeDirection = Random.insideUnitSphere;
                fleeDirection.y = 0f; // Keep on ground plane
                fleeDirection.Normalize();
                
                // Play flee animation
                TroopView view = GetComponentInParent<TroopView>();
                if (view != null)
                {
                    view.PlayFleeAnimation(null);
                }
            }
        }
        
        public override void OnDrawGizmos()
        {
            if (Time.time < fearEndTime)
            {
                Gizmos.color = Color.yellow;
                
                if (isHesitating)
                {
                    Gizmos.DrawWireSphere(transform.position, 1f);
                }
                else
                {
                    Gizmos.DrawRay(transform.position, fleeDirection * 3f);
                }
            }
        }
    }