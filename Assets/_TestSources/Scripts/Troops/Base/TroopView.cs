using System;
using System.Collections;
using IslandDefense.Troops;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View class for troops. Handles visuals, animations, and effects.
/// Follows MVC pattern as the View component.
/// </summary>
public class TroopView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TroopBase troopBase;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform modelTransform;
        
    [Header("UI Elements")]
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private GameObject selectionIndicator;
        
    [Header("Effects")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private ParticleSystem attackEffect;
        
    // Animation parameter names
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private static readonly int IsDefending = Animator.StringToHash("IsDefending");
    private static readonly int IsFleeing = Animator.StringToHash("IsFleeing");
    private static readonly int TriggerAttack = Animator.StringToHash("TriggerAttack");
    private static readonly int TriggerHit = Animator.StringToHash("TriggerHit");
    private static readonly int TriggerDeath = Animator.StringToHash("TriggerDeath");
    private static readonly int TriggerFear = Animator.StringToHash("TriggerFear");
    private static readonly int MoveType = Animator.StringToHash("MoveType");
        
    private void Awake()
    {
        // Ensure references are set
        if (troopBase == null)
            troopBase = GetComponent<TroopBase>();
                
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
                
        if (modelTransform == null && animator != null)
            modelTransform = animator.transform;
                
        // Initialize UI
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = Camera.main;
        }
            
        SetSelection(false);
    }
        
    private void Start()
    {
        // Initialize health bar
        UpdateHealthBar(troopBase.Health, troopBase.MaxHealth);
    }
        
    /// <summary>
    /// Update the health bar UI
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
                
            // Show/hide health bar based on health
            healthBarCanvas.gameObject.SetActive(currentHealth < maxHealth);
        }
    }
        
    /// <summary>
    /// Show/hide selection indicator
    /// </summary>
    public void SetSelection(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }
        
    #region Animation Methods
        
    /// <summary>
    /// Play idle animation
    /// </summary>
    public void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(IsMoving, false);
            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsDefending, false);
            animator.SetBool(IsFleeing, false);
            animator.SetInteger(MoveType, 0);
        }
    }
        
    /// <summary>
    /// Play movement animation
    /// </summary>
    public void PlayMoveAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(IsMoving, true);
            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsDefending, false);
            animator.SetBool(IsFleeing, false);
            animator.SetInteger(MoveType, 0);
        }
    }
        
    /// <summary>
    /// Play cautious movement animation (for approaching enemies)
    /// </summary>
    public void PlayCautiousMoveAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(IsMoving, true);
            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsDefending, false);
            animator.SetBool(IsFleeing, false);
            animator.SetInteger(MoveType, 1); // Cautious movement type
        }
    }
        
    /// <summary>
    /// Play combat idle animation
    /// </summary>
    public void PlayCombatIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(IsMoving, false);
            animator.SetBool(IsAttacking, true);
            animator.SetBool(IsDefending, false);
            animator.SetBool(IsFleeing, false);
        }
    }
        
    /// <summary>
    /// Play attack animation with callback
    /// </summary>
    public void PlayAttackAnimation(Action onAnimationComplete = null)
    {
        if (animator != null)
        {
            animator.SetTrigger(TriggerAttack);
                
            if (attackEffect != null)
            {
                attackEffect.Play();
            }
        }
            
        // Since we don't have animation events hooked up in this prototype,
        // use a coroutine to estimate when the animation finishes
        if (onAnimationComplete != null)
        {
            StartCoroutine(WaitForAnimationComplete(1.0f, onAnimationComplete));
        }
    }
        
    /// <summary>
    /// Play hit reaction animation
    /// </summary>
    public void PlayHitEffect()
    {
        if (animator != null)
        {
            animator.SetTrigger(TriggerHit);
        }
            
        if (hitEffect != null)
        {
            hitEffect.Play();
        }
    }
        
    /// <summary>
    /// Play death animation with callback
    /// </summary>
    public void PlayDeathAnimation(Action onAnimationComplete = null)
    {
        if (animator != null)
        {
            animator.SetTrigger(TriggerDeath);
            animator.SetBool(IsDead, true);
        }
            
        if (deathEffect != null)
        {
            deathEffect.Play();
        }
            
        // Hide health bar
        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }
            
        // Wait for animation to complete
        if (onAnimationComplete != null)
        {
            StartCoroutine(WaitForAnimationComplete(2.0f, onAnimationComplete));
        }
    }
        
    /// <summary>
    /// Play defend animation
    /// </summary>
    public void PlayDefendAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(IsDefending, true);
            animator.SetBool(IsMoving, false);
            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsFleeing, false);
        }
    }
        
    /// <summary>
    /// Play flee animation
    /// </summary>
    public void PlayFleeAnimation(Action onAnimationComplete = null)
    {
        if (animator != null)
        {
            animator.SetBool(IsFleeing, true);
            animator.SetBool(IsMoving, true);
            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsDefending, false);
        }
            
        if (onAnimationComplete != null)
        {
            StartCoroutine(WaitForAnimationComplete(1.5f, onAnimationComplete));
        }
    }
        
    /// <summary>
    /// Play fear animation
    /// </summary>
    public void PlayFearAnimation(Action onAnimationComplete = null)
    {
        if (animator != null)
        {
            animator.SetTrigger(TriggerFear);
        }
            
        if (onAnimationComplete != null)
        {
            StartCoroutine(WaitForAnimationComplete(1.0f, onAnimationComplete));
        }
    }
        
    #endregion
        
    /// <summary>
    /// Wait for an animation to complete and then trigger a callback
    /// </summary>
    private IEnumerator WaitForAnimationComplete(float estimatedDuration, Action callback)
    {
        yield return new WaitForSeconds(estimatedDuration);
        callback?.Invoke();
    }
}