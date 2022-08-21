using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Start is called before the first frame update
    public DesignManager designManager;
    public TMP_Text totalPriceText;
    private float betValue;
    private float totalValue;
    public TMP_Text alertText;
    private int loop = 0;
    private string FONflag;
    public Button betbtn;
    public Button increasebtn;
    public Button decreasebtn;
    public Button foldbtn;
    public TMP_InputField inputPriceText;

    public static APIForm apiform;
    public static Globalinitial _global;
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);
    BetPlayer _player;
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];
        float i_balance = float.Parse(usersInfo["amount"]);
        totalValue = i_balance;
        totalPriceText.text = totalValue.ToString("F2");
    }

    void Start()
    {
        _player = new BetPlayer();
#if UNITY_WEBGL == true && UNITY_EDITOR == false
                    GameReady("Ready");
#endif
        StartCoroutine(firstServer());
        designManager = FindObjectOfType<DesignManager>();
        betValue = 10f;
        inputPriceText.text = betValue.ToString("f2");
        betbtn.interactable = false;
    }
    // Update is called once per frame
    void Update()
    {

    }
    public void BetOrRebet()
    {
        if (totalValue >= betValue)
        {
            if (totalValue >= 10)
            {
                increasebtn.interactable = false;
                decreasebtn.interactable = false;
                inputPriceText.interactable = false;
                switch (loop)
                {
                    case 0:
                        StartCoroutine(cardActiveClear());
                        betbtn.interactable = false;
                        foldbtn.interactable = false;
                        StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - betValue));
                        StartCoroutine(designManager.CardThrow(0, 7, true));
                        foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "FOLD";
                        FONflag = "FOLD";
                        betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "CONTINUE";
                        loop = loop + 1;
                        StartCoroutine(beginServer());
                        betValue = 2 * betValue;
                        inputPriceText.text = betValue.ToString("f2");
                        break;
                    case 1:
                        StartCoroutine(cardActiveClear());
                        betbtn.interactable = false;
                        foldbtn.interactable = false;
                        StartCoroutine(designManager.CardThrow(7, 9, false));
                        StartCoroutine(designManager.CardRotate(2, 4));
                        StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - betValue));
                        foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "NEW BET";
                        FONflag = "NEW BET";
                        betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "REBET";
                        loop = loop + 1;
                        StartCoroutine(Server());
                        betValue = betValue / 2;
                        inputPriceText.text = betValue.ToString("f2");
                        break;
                    case 2:
                        StartCoroutine(cardActiveClear());
                        betbtn.interactable = false;
                        foldbtn.interactable = false;
                        StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - betValue));
                        StartCoroutine(designManager.ThrowedCardClear(true));
                        foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "FOLD";
                        FONflag = "FOLD";
                        betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "CONTINUE";
                        loop = 1;
                        betValue = 2 * betValue;
                        inputPriceText.text = betValue.ToString("f2");
                        break;
                }
            }
            else
            {
                StartCoroutine(alert("Insufficient balance!", "other"));
            }
        }
        else
        {
            StartCoroutine(alert("Insufficient balance!", "other"));
        }
    }
    public void NewOrFold()
    {
        foldbtn.interactable = false;
        betbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "BET";
        loop = 0;
        switch (FONflag)
        {
            case "FOLD":
                StartCoroutine(HoldServer());
                foldbtn.transform.GetChild(0).GetComponent<TMP_Text>().text = "NEW BET";
                FONflag = "NEW BET";
                break;
            case "NEW BET":
                firstStatus();
                break;
        }
        StartCoroutine(designManager.ThrowedCardClear(false));
    }
    public void firstStatus()
    {
        inputPriceText.interactable = true;
        betValue = 10;
        inputPriceText.text = betValue.ToString("f2");
        if (betValue <= 10)
        {
            decreasebtn.interactable = false;
            increasebtn.interactable = true;
        }
        else if (betValue >= 100000)
        {
            increasebtn.interactable = false;
            decreasebtn.interactable = true;
        }
        else
        {
            increasebtn.interactable = true;
            decreasebtn.interactable = true;
        }
    }
    public IEnumerator firstServer()
    {
        yield return new WaitForSeconds(0.5f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/CardOder", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                designManager.cardOrderArray = apiform.cardOder;
                StartCoroutine(designManager.CardOder());
                yield return new WaitForSeconds(2.5f);
                betbtn.interactable = true;
                foldbtn.interactable = true;
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
        }
    }
    public IEnumerator beginServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("token", _player.token);
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/bet-holdem", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                yield return new WaitForSeconds(1f);
                for (int i = 0; i < apiform.activeArray.Length; i++)
                {
                    if (apiform.activeArray[i] == true)
                    {
                        string name = "";
                        if (i < 2)
                        {
                            name = "card" + (i + 1);
                        }
                        else
                        {
                            name = "card" + (i + 3);
                        }
                        GameObject.Find(name).GetComponent<SpriteRenderer>().material.color = Color.yellow;
                    }
                }
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue));
        }
    }
    IEnumerator Server()
    {
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("betAmount", betValue.ToString("F2"));
        form.AddField("token", _player.token);
        form.AddField("amount", totalValue.ToString("F2"));
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/result-holdem", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                yield return new WaitForSeconds(1.5f);
                for (int i = 0; i < apiform.activeArray.Length; i++)
                {
                    if (apiform.activeArray[i] == true)
                    {
                        string name = "card" + (i + 1);
                        GameObject.Find(name).GetComponent<SpriteRenderer>().material.color = Color.yellow;
                    }
                }
                yield return new WaitForSeconds(1f);
                if (apiform.result == "user")
                {
                    StartCoroutine(alert(apiform.msg, "win"));
                    StartCoroutine(UpdateCoinsAmount(totalValue, apiform.total));
                }
                else
                {
                    StartCoroutine(alert(apiform.msg, "lose"));
                }

            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + 1.5f * betValue));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + 1.5f * betValue));
        }
    }
    IEnumerator HoldServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("betAmount", betValue.ToString("F2"));
        form.AddField("token", _player.token);
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/result-fold", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                StartCoroutine(alert("You fold", "lose"));
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue / 2));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue / 2));
        }
    }
    public void halfControll()
    {
        betValue = betValue / 2;
        inputPriceText.text = betValue.ToString("F2");
    }
    public void doubleControll()
    {
        if (totalValue >= 2 * betValue)
        {
            betValue = betValue * 2;
            inputPriceText.text = betValue.ToString("F2");
        }
    }
    public void inputChanged()
    {
        betValue = float.Parse(string.IsNullOrEmpty(inputPriceText.text) ? "0" : inputPriceText.text);
        if (betValue <= 10)
        {
            betValue = 10;
            inputPriceText.text = betValue.ToString("F2");
            decreasebtn.interactable = false;
            increasebtn.interactable = true;
        }
        else if (betValue >= 100000)
        {
            betValue = 100000;
            inputPriceText.text = betValue.ToString("F2");
            increasebtn.interactable = false;
            decreasebtn.interactable = true;
        }
        else
        {
            increasebtn.interactable = true;
            decreasebtn.interactable = true;
        }
    }
    public IEnumerator alert(string msg, string state)
    {
        if (state == "win")
        {
            AlertController.isWin = true;
        }
        else
        {
            AlertController.isLose = true;
        }
        alertText.text = msg;
        yield return new WaitForSeconds(2f);
        AlertController.isWin = false;
        AlertController.isLose = false;
        yield return new WaitForSeconds(2.5f);
        betbtn.interactable = true;
        foldbtn.interactable = true;
    }
    private IEnumerator UpdateCoinsAmount(float preValue, float changeValue)
    {
        // Animation for increasing and decreasing of coins amount
        const float seconds = 0.2f;
        float elapsedTime = 0;
        while (elapsedTime < seconds)
        {
            totalPriceText.text = Mathf.Floor(Mathf.Lerp(preValue, changeValue, (elapsedTime / seconds))).ToString();
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        totalValue = changeValue;
        totalPriceText.text = totalValue.ToString();
    }
    public IEnumerator cardActiveClear()
    {
        for (int i = 0; i < 10; i++)
        {
            string name = "card" + (i + 1);
            GameObject.Find(name).GetComponent<SpriteRenderer>().material.color = Color.white;
        }
        yield return new WaitForSeconds(1f);
    }
}
public class BetPlayer
{
    public string username;
    public string token;
}
