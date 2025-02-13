using System;
using System.Collections.Generic;
using UnityEngine;

public class BlobTrigger : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] BlobJoint blobJoint;
    
    List<GameObject> collisions = new();
    Dictionary<GameObject, ContactPoint2D> lastContacts = new();

    //[Space(10)]
    // RSO
    // RSF
    // RSP

    //[Header("Input")]
    //[Header("Output")]
    public Action<Collision2D> OnCollisionEnter, OnCollisionExit;
    public Action<ContactPoint2D> OnCollisionExitGetLastContact;

    private void OnDisable()
    {
        blobJoint.RemoveOnCollisionEnterListener(OnEnterCollision);
        blobJoint.RemoveOnCollisionExitListener(OnExitCollision);
    }

    private void Start()
    {
        Invoke("LateStart", .05f);
    }
    void LateStart()
    {
        blobJoint.AddOnCollisionEnterListener(OnEnterCollision);
        blobJoint.AddOnCollisionExitListener(OnExitCollision);
    }

    void OnEnterCollision(Collision2D collision)
    {
        if (!collisions.Contains(collision.gameObject))
        {
            OnCollisionEnter?.Invoke(collision);
        }
        collisions.Add(collision.gameObject);

        // Stocke le dernier point de contact et sa normale
        if (collision.contactCount > 0)
        {
            lastContacts[collision.gameObject] = collision.GetContact(collision.contactCount - 1);
        }
    }

    void OnExitCollision(Collision2D collision)
    {
        collisions.Remove(collision.gameObject);

        if (!collisions.Contains(collision.gameObject))
        {
            OnCollisionExit?.Invoke(collision);

            if (lastContacts.TryGetValue(collision.gameObject, out ContactPoint2D lastPoint))
            {
                OnCollisionExitGetLastContact?.Invoke(lastPoint);
            }

            lastContacts.Remove(collision.gameObject);
        }
    }
}