using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BlobCombat : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float pushBackForce;
    [SerializeField] float returnPushBackForce;
    [SerializeField] float extendForceMultiplier;

    [Header("References")]
    [SerializeField] BlobTrigger blobTrigger;
    [SerializeField] BlobJoint blobJoint;
    [SerializeField] BlobMovement blobMovement;

    LayerMask currentLayer;

    List<BlobMotor> blobsTouch = new();

    //[Space(10)]
    // RSO
    // RSF
    // RSP

    //[Header("Input")]
    //[Header("Output")]

    private void OnDisable()
    {
        blobTrigger.OnCollisionEnterWithBlob -= OnBlobCollisionEnter;
    }

    private void Start()
    {
        Invoke("LateStart", .05f);
    }
    void LateStart()
    {
        blobTrigger.OnCollisionEnterWithBlob += OnBlobCollisionEnter;
    }

    void OnBlobCollisionEnter(BlobMotor blob)
    {
        if (blobsTouch.Contains(blob)) return;

        float velocity = blobJoint.GetVelocity().sqrMagnitude;
        Vector2 impactDir = (blob.GetJoint().GetJointsCenter() - blobJoint.GetJointsCenter()).normalized;

        if (!blob.GetMovement().IsExtend() && blobMovement.IsExtend())
        {
            Vector2 propulsionDir = CalculateExpulsionDirection(blob.GetTrigger().GetCollisions(), impactDir);
            Debug.DrawLine(blob.GetJoint().GetJointsCenter(), blob.GetJoint().GetJointsCenter() + propulsionDir, Color.blue, 1);

            Vector2 impactForce = propulsionDir * pushBackForce * velocity * extendForceMultiplier;
            blob.GetJoint().AddForce(impactForce);

            blobJoint.AddForce(-impactDir * returnPushBackForce * velocity * blob.GetHealth().GetPercentage());

            blob.GetHealth().AddPercentageWithSpeed(impactForce.sqrMagnitude);
            blob.GetTrigger().ExludeLayer(currentLayer, .1f);

            StartCoroutine(ImpactCooldown(blob));
        }
        else if(!blob.GetMovement().IsExtend() && !blobMovement.IsExtend())
        {
            Vector2 impactForce = impactDir * pushBackForce * velocity;
            blob.GetJoint().AddForce(impactForce * blob.GetHealth().GetPercentage());
            blob.GetHealth().AddPercentageWithSpeed(impactForce.sqrMagnitude);

            blobJoint.AddForce(-impactDir * returnPushBackForce * velocity);

            StartCoroutine(ImpactCooldown(blob));
        }
    }
    IEnumerator ImpactCooldown(BlobMotor blobTouch)
    {
        blobsTouch.Add(blobTouch);
        yield return new WaitForSeconds(.1f);
        blobsTouch.Remove(blobTouch);
    }

    public static Vector2 CalculateExpulsionDirection(List<Collision2D> worldCollisions, Vector2 impactDirection)
    {
        if (worldCollisions == null || worldCollisions.Count == 0)
            return impactDirection.normalized;

        Vector2 totalNormal = Vector2.zero;

        foreach (var collision in worldCollisions)
        {
            foreach (var contact in collision.contacts)
            {
                totalNormal += contact.normal;
            }
        }
        totalNormal.Normalize();

        Vector2 blockedComponent = Vector2.Dot(impactDirection, totalNormal) * totalNormal;
        Vector2 freeComponent = impactDirection - blockedComponent;

        if (freeComponent.magnitude < 0.1f)
        {
            freeComponent = new Vector2(-totalNormal.y, totalNormal.x); // Rotation 90�
        }

        return freeComponent.normalized;
    }

    public void SetLayer(LayerMask layer)
    {
        currentLayer = layer;
    }
}