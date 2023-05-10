using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WalletLogin: MonoBehaviour
{
    public Text info; 
    public GameObject networkManager;
    NetworkManager NetworkManager;
    void Start() {
        NetworkManager = networkManager.GetComponent<NetworkManager>();
        // if remember me is checked, set the account to the saved account
        //rememberMe.isOn &&
        if (PlayerPrefs.GetString("Account") != "")
        {
            // move to next scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    async public void OnLogin()
    {
        info.text = "Signing In..."; 
        // get current timestamp
        int timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        // set expiration time
        int expirationTime = timestamp + 60;
        // set message
        string message = expirationTime.ToString();
        // sign message
        string signature = await Web3Wallet.Sign(message);
        // verify account
        string account = await EVM.Verify(message, signature);
        int now = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        // validate
        if (account.Length == 42 && expirationTime >= now) 
        {
            // save account
            info.text = "Signed In."; 
            Debug.Log("Signed In");
            //PlayerPrefs.SetString("Account", account);
            // load next scene
            networkManager.SetActive(true);
            NetworkManager.message = message;
            NetworkManager.userAddress = account;
            NetworkManager.userSignature = signature;
            
            Debug.Log(signature);
            info.text = message + " " + account; 
        }
        else
        {
            info.text = "Failed! Trying Again."; 
            OnLogin();
        }
    }
}