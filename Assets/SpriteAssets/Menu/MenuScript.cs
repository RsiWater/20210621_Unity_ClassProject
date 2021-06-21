using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    [Header("標題文字物件")]
    public UnityEngine.UI.Text titleText;
    [Header("描述文字物件")]

    public UnityEngine.UI.Text descriptionText;
    private bool isPause = false, isGameOver = false;
    void Awake()
    {
        this.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        // Debug.Log("test");
        // if(Input.GetKey(KeyCode.T))
        // {
        //     this.Pause();
        // }
        if(isGameOver && Input.GetKey(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            this.isPause = false;
            this.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void Pause()
    {
        this.isPause = !this.isPause;
        this.gameObject.SetActive(this.isPause);
        Time.timeScale = this.isPause ? 0 : 1;
        if(this.isPause)Debug.Log("Pause");
        else Debug.Log("gogo");
    }
    public void Defeated()
    {
        this.isPause = true;
        this.gameObject.SetActive(true);
        Time.timeScale = 0;

        this.isGameOver = true;
        this.titleText.text = "Defeated..";
        this.descriptionText.text = "Press R to retry";
    }

    public void Victory()
    {
        this.isPause = true;
        this.gameObject.SetActive(true);
        Time.timeScale = 0;

        this.isGameOver = true;
        this.titleText.text = "Victory!";
        this.descriptionText.text = "Press R to play again";
    }

    public void resetAllTheGame()
    {
        
    }
}
