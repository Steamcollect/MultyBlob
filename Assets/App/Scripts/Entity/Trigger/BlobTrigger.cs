using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class BlobTrigger : CollisionTrigger
{
    [Header("Settings")]
    [SerializeField, TagName] private string groundableTag;
    [SerializeField, TagName] private string slidableTag;
    [SerializeField, TagName] private string windTag;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool isSliding = false;
    [SerializeField] private bool isInWind = false;

    [Header("References")]
    [SerializeField] private BlobPhysics physics;

    public Action<Collision2D> OnGroundedEnter, OnGroundedExit;
    public Action OnGroundTouch;
    public Action<Collision2D> OnSlidableEnter, OnSlidableExit;
    public Action _OnWindEnter;

    private List<GameObject> groundables = new();
    private List<GameObject> slidables = new();
    private int windsTouchCount = 0;
    private LayerMask layerToExclude;

    private void OnDisable()
    {
        physics.RemoveOnCollisionEnterListener(OnEnterCollision);
        physics.RemoveOnCollisionExitListener(OnExitCollision);
        physics.RemoveOnCollisionStayListener(OnStayCollision);

        OnCollisionEnter -= OnEnter;
        OnCollisionExit -= OnExit;
    }

    private void Start()
    {
        Invoke(nameof(LateStart), 0.1f);
    }

    private void LateStart()
    {
        physics.AddOnCollisionEnterListener(OnEnterCollision);
        physics.AddOnCollisionExitListener(OnExitCollision);
        physics.AddOnCollisionStayListener(OnStayCollision);

        OnCollisionEnter += OnEnter;
        OnCollisionExit += OnExit;
    }

    private void OnEnter(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundableTag))
        {
            isGrounded = true;

            OnGroundedEnter?.Invoke(collision);
            OnGroundTouch?.Invoke();
            groundables.Add(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag(slidableTag))
        {
            isSliding = true;

            OnSlidableEnter?.Invoke(collision);
            slidables.Add(collision.gameObject);
        }
    }

    private void OnExit(Collision2D collision)
    {
        groundables.Remove(collision.gameObject);
        if(groundables.Count <= 0)
        {
            isGrounded = false;
            OnGroundedExit?.Invoke(collision);
        }

        slidables.Remove(collision.gameObject);
        if(slidables.Count <= 0)
        {
            isSliding = false;
            OnSlidableExit?.Invoke(collision);
        }
    }

    public void OnWindEnter()
    {
        windsTouchCount++;
        isInWind = true;

        _OnWindEnter.Invoke();
    }

    public void OnWindExit()
    {
        windsTouchCount--;
        if (windsTouchCount < 0) windsTouchCount = 0;

        if(windsTouchCount <= 0)
        {
            isInWind = false;
        }
    }

    public bool IsGrounded() { return isGrounded; }

    public bool IsSliding() { return isSliding; }

    public bool IsInWind() { return isInWind; }

    public void ExludeLayer(LayerMask layerToExclude, float excludingTime)
    {
        LayerMask combine = this.layerToExclude | layerToExclude;
        this.layerToExclude = combine;
        physics.SetLayerToExlude(this.layerToExclude);

        StartCoroutine(RemoveExludeLayer(layerToExclude, excludingTime));
    }

    private IEnumerator RemoveExludeLayer(LayerMask layerToExclude, float excludingTime)
    {
        yield return new WaitForSeconds(excludingTime);
        this.layerToExclude = this.layerToExclude & ~layerToExclude;
        physics.SetLayerToExlude(this.layerToExclude);
    }

    public void SetLayerToExclude(LayerMask layerToExclude)
    {
        this.layerToExclude = layerToExclude;
    }

    public void ResetTouchs()
    {
        groundables.Clear();
        slidables.Clear();
        isGrounded = false;
        isSliding = false;
        isInWind = false;
        windsTouchCount = 0;
    }
}