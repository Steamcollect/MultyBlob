using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class TransitionLevel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool doStart;
    [SerializeField] private float delayStart;
    [SerializeField] private bool modeDev;
    [SerializeField] private List<Color> colorBlob;

    [Header("Input")]
    [SerializeField] private RSE_OnFightEnd rseOnFightEnd;
    [SerializeField] private RSE_Transit rseTransit;
    [SerializeField] private RSE_OnPause rseOnPause;
    [SerializeField] private RSE_OnResume rseOnResume;

    [Header("Output")]
    [SerializeField] private RSE_LoadNextLevel rseLoadNextLevel;
    [SerializeField] private RSE_OnFightStart rseOnFightStart;
    [SerializeField] private RSE_SpawnPoint rseSpawnPoint;
    [SerializeField] private RSE_Message rseMessage;
    [SerializeField] private SSO_ListFightText listFightText;
    [SerializeField] private RSO_BlobInGame rsoBlobInGame;

    private bool isPaused = false;
    private int randomReadyText = 0;
    private int randomStartText = 0;

    private void OnEnable()
    {
        rseOnFightEnd.action += TransitionEnd;
        rseTransit.action += TransitionEnd;
        rseOnPause.action += Pause;
        rseOnResume.action += Resume;
    }

    private void OnDisable()
    {
        rseOnFightEnd.action -= TransitionEnd;
        rseTransit.action -= TransitionEnd;
        rseOnPause.action -= Pause;
        rseOnResume.action -= Resume;
    }

    private void Pause()
    {
        isPaused = true;
        gameObject.transform.DOPause();
    }

    private void Resume()
    {
        isPaused = false;
        gameObject.transform.DOPlay();
    }

    private void Start()
    {
        if (doStart)
        {
            gameObject.transform.position = new Vector3(-100, 0, -10);

            StartCoroutine(Utils.Delay(0.1f, () =>
            {
                rseSpawnPoint.Call();

                for (int i = 0; i < rsoBlobInGame.Value.Count; i++)
                {
                    rsoBlobInGame.Value[i].GetVisual().SetColor(colorBlob[i]);
                }
            }));

            gameObject.transform.DOMove(new Vector3(0, 0, transform.position.z), 0.6f).OnComplete(() =>
            {
                randomStartText = Random.Range(0, listFightText.startText.Count);
                if (modeDev)
                {
                    rseMessage.Call($"{listFightText.startText[randomStartText]}", 0.5f, listFightText.colorMessage, MessageManager.MessageTexteType.Start);
                    rseOnFightStart.Call();
                }
                else
                {
                    StartCoroutine(DelayStart());
                }
            });
        }
    }

    private IEnumerator DelayStart()
    {
        float cooldown = delayStart;
        float timer = 0f;
        randomReadyText = Random.Range(0, listFightText.readyText.Count);
        rseMessage.Call($"{listFightText.readyText[randomReadyText]}", 0.5f, listFightText.colorMessage,MessageManager.MessageTexteType.Ready);

        while (timer < cooldown)
        {
            yield return null;

            if (!isPaused)
            {
                timer += Time.deltaTime;
            }
        }
        rseMessage.Call($"{listFightText.startText[randomStartText]}", 0.5f, listFightText.colorMessage, MessageManager.MessageTexteType.Start);

        rseOnFightStart.Call();
    }

    private void TransitionEnd()
    {
        StartCoroutine(DelayEnd());
    }

    private IEnumerator DelayEnd()
    {
        float cooldown = 1f;
        float timer = 0f;

        while (timer < cooldown)
        {
            yield return null;

            if (!isPaused)
            {
                timer += Time.deltaTime;
            }
        }

        if (doStart)
        {
            gameObject.transform.DOMove(transform.position + new Vector3(10, 0, 0), 0.4f).OnComplete(() =>
            {
                gameObject.transform.DOMove(new Vector3(-100, transform.position.y, transform.position.z), 1f).OnComplete(() =>
                {
                    rseLoadNextLevel.Call();
                });
            });
        }
        else
        {
            gameObject.transform.DOMove(transform.position - new Vector3(10, 0, 0), 0.4f).OnComplete(() =>
            {
                gameObject.transform.DOMove(new Vector3(100, transform.position.y, transform.position.z), 1f).OnComplete(() =>
                {
                    rseLoadNextLevel.Call();
                });
            });
        }
    }
}