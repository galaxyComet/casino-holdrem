using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.SceneManagement;
using SimpleJSON;
public class DesignManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("material")]
    private Transform CardObject;
    public Material[] cardMaterial;
    public Transform prefab;
    public GameManager gameManager;
    private float[] cardX;
    private float cardY = -305.954f;
    private float cardZ = -882.74f;
    private float[] movecardX = new float[9] { 1676.928f, 1677.077f, 1676.927f, 1677.078f, 1676.701f, 1676.851f, 1677.001f, 1677.151f, 1677.301f };
    private float movecardY = -305.9988f;
    private float[] movecardZ = new float[9] { -883.2369f, -883.2369f, -882.806f, -882.806f, -883.0248f, -883.0248f, -883.0248f, -883.0248f, -883.0248f };
    public int[] cardOrderArray;
    void Start()
    {
        cardX = new float[9];
        gameManager = FindObjectOfType<GameManager>();
    }

    public IEnumerator CardOder()
    {
        const float seconds = 0.1f;
        float time = 0;
        float before = 1677.483f;
        cardX[0] = before;
        while (time < seconds)
        {
            for (int i = 0; i < 52; i++)
            {
                float next = before + 0.0045f * i;
                CardObject = Instantiate(prefab, Vector3.Lerp(new Vector3(before, cardY, cardZ), new Vector3(next, cardY, cardZ), time / seconds), Quaternion.identity);
                if (i > 0 && i < 9)
                {
                    cardX[i] = next;
                }
                CardObject.name = "card" + (i + 1);
                CardObject.GetComponent<SpriteRenderer>().material = cardMaterial[cardOrderArray[i]];
                CardObject.transform.localScale = new Vector3(0.008599492f, -0.008231715f, 0.65701f);
                CardObject.transform.eulerAngles = new Vector3(30, 90, 90);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
    public IEnumerator CardThrow(int from, int to, bool flag)
    {
        for (int i = from; i < to; i++)
        {
            float time = 0;
            const float seconds = 0.15f;
            string name = "card" + (i + 1);
            while (time < seconds)
            {
                GameObject.Find(name).transform.position = Vector3.Lerp(new Vector3(cardX[i], cardY, cardZ), new Vector3(movecardX[i], movecardY, movecardZ[i]), time / seconds);
                if (i > 1 && i < 4)
                {
                    GameObject.Find(name).transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(30, 90, 90)), Quaternion.Euler(new Vector3(90, 90, 90)), time / seconds);
                }
                else
                {
                    GameObject.Find(name).transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(30, 90, 90)), Quaternion.Euler(new Vector3(-90, 90, 90)), time / seconds);
                }
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            GameObject.Find(name).transform.position = new Vector3(movecardX[i], movecardY, movecardZ[i]);
            if (i > 1 && i < 4)
            {
                GameObject.Find(name).transform.rotation = Quaternion.Euler(new Vector3(90, 90, 90));
            }
            else
            {
                GameObject.Find(name).transform.rotation = Quaternion.Euler(new Vector3(-90, 90, 90));
            }

        }
        if (flag)
        {
            yield return new WaitForSeconds(1f);
            gameManager.betbtn.interactable = true;
            gameManager.foldbtn.interactable = true;
        }

    }
    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator ThrowedCardClear(bool flag)
    {
        for (int i = 0; i < 52; i++)
        {
            string name = "card" + (i + 1);
            Destroy(GameObject.Find(name));
        }
        StartCoroutine(gameManager.firstServer());
        yield return new WaitForSeconds(1.5f);
        if (flag)
        {
            StartCoroutine(CardThrow(0, 7, true));
            StartCoroutine(gameManager.beginServer());
        }
    }
    public IEnumerator CardRotate(int from, int to)
    {
        yield return new WaitForSeconds(1f);
        for (int i = from; i < to; i++)
        {
            float time = 0;
            const float seconds = 0.15f;
            string name = "card" + (i + 1);
            while (time < seconds)
            {
                GameObject.Find(name).transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(90, 90, 90)), Quaternion.Euler(new Vector3(-90, 90, 90)), time / seconds);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            GameObject.Find(name).transform.rotation = Quaternion.Euler(new Vector3(-90, 90, 90));
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
    }
}
