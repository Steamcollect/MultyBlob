using UnityEngine;
using UnityEngine.InputSystem;

public class BlobHealth : EntityHealth, IPausable
{
    [Header("Settings")]
    float pushBackPercentage;
    [SerializeField] AnimationCurve percentagePerSpeedOnImpactCurve;
    [SerializeField] AnimationCurve stunTimePerSpeedOnImpactCurve;

    [Space(5)]
    [SerializeField] int maxImpactSpeed;

    [Space(10)]
    [SerializeField] float shakeIntensityOnDeath;
    [SerializeField] float shakeTimeOnDeath;

    [Header("References")] 
    [SerializeField] BlobTrigger blobTrigger;
    [SerializeField] BlobMovement blobMovement;
    [SerializeField] BlobParticle particle;
    [SerializeField] PlayerInput playerInput;

    [Space(10)]
    [SerializeField] BlobPercentageEffect percentageEffect;
    [SerializeField] RSE_CallRumble rSE_CallRumble;
    //[Space(10)]
    // RSO
    // RSF
    // RSP

    //[Header("Input")]
    //[Header("Output")]

    [SerializeField] RSE_CameraShake rseCamShake;

    private void OnDisable()
    {
        blobTrigger.OnCollisionEnter -= OnEnterCollision;
        onDeath -= OnDeath;
        //onTakeDamage -= OnTakeDamage;
    }

    private void Start()
    {
        Setup();
        Invoke("LateStart", .05f);
        
    }
    void LateStart()
    {
        blobTrigger.OnCollisionEnter += OnEnterCollision;
        onDeath += OnDeath;
        //onTakeDamage += OnTakeDamage;
    }

    public void Setup()
    {
        isDead = false;
        currentHealth = maxHealth;

        pushBackPercentage = 0;
    }

    void OnDeath()
    {
        pushBackPercentage = 0;
        rseCamShake.Call(shakeIntensityOnDeath, shakeTimeOnDeath);
        rSE_CallRumble.Call(playerInput.user.index, playerInput.currentControlScheme);
    }

    void OnEnterCollision(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Damagable damagable))
        {
            switch (damagable.GetDamageType())
            {
                case Damagable.DamageType.Damage:
                    TakeDamage(damagable.GetDamage());
                    break;
                
                case Damagable.DamageType.Kill:
                    rseCamShake.Call(shakeIntensityOnDeath, shakeTimeOnDeath);
                    rSE_CallRumble.Call(playerInput.user.index, playerInput.currentControlScheme);
                    Die();
                    break;
                
                case Damagable.DamageType.Destroy:
                    if (isDead) return;
                    rseCamShake.Call(shakeIntensityOnDeath, shakeTimeOnDeath);
                    rSE_CallRumble.Call(playerInput.user.index, playerInput.currentControlScheme);
                    Destroy(collision);
                    break;
            }
        }
    }

    public void Pause()
    {
        isInvincible = true;
    }
    public void Resume()
    {
        isInvincible = false;
    }

    public void OnDamageImpact(float speed)
    {
        speed = Mathf.Clamp01(speed / maxImpactSpeed);

        blobMovement.StunImpact(stunTimePerSpeedOnImpactCurve.Evaluate(speed));
        pushBackPercentage += (percentagePerSpeedOnImpactCurve.Evaluate(speed));

        percentageEffect.Setup(pushBackPercentage);
    }
    public float GetPercentage() { return 1 + pushBackPercentage; }
}