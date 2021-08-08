using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class IntroManager : MonoBehaviour
{
    //prefabs + instances
    public Bird birdPrefab;
    private Bird birdInstance;

    private int time;

    //ui
    public Text titleText;
    public Text introText;

    public Text explanationText;

    //for the couroutines that show and hide text
    private bool coOver = false;
    private int done = 0;

    // Start is called before the first frame update
    void Start()
    {
        time = 0;
        //creates the bird
        birdInstance = Instantiate(birdPrefab) as Bird;
        birdInstance.transform.localScale = new Vector2(0.15f, 0.15f);
        birdInstance.transform.position = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        time++;
        //move the bird
        if(time % 7 == 0 && !birdInstance.getFalling()) //time repeats every 7 ticks and the bird is currently going up
        {
            birdInstance.upBird();
        }

        if(birdInstance.transform.position.y >= 1.5f) //too high, needs to fall
        {
            birdInstance.cancelUp();
        }

        if(birdInstance.transform.position.y <= -1.5f)//too low, needs to fly
        {
            birdInstance.cancelFalling();
        }

        //checks for moving onto the next stage

        if (Input.GetKeyDown(KeyCode.Space) && done == 0)
        {
            StartCoroutine(FadeImage(true, titleText));
            StartCoroutine(FadeImage(true, introText));
        }

        if(coOver && done == 2)
        {
            coOver = false;
            StartCoroutine(FadeImage(false, explanationText));
            birdInstance.gameObject.SetActive(false);
        }

        if(done == 3 && Input.GetKeyDown(KeyCode.Space)) //move onto the game scene
        {
            Debug.Log("yeet");
            SceneManager.LoadScene(1);
        }

    }


    //for fading in and out

    //source: https://forum.unity.com/threads/simple-ui-animation-fade-in-fade-out-c.439825/
    IEnumerator FadeImage(bool fadeAway, Text text)
    {
        // fade from opaque to transparent
        if (fadeAway)
        {
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha
                text.color = new Color(0, 0, 0, i);
                yield return null;               
            }
            coOver = true;
            done++;
        }
        // fade from transparent to opaque
        else
        {
            // loop over 1 second
            for (float i = 0; i <= 1; i += Time.deltaTime)
            {
                // set color with i as alpha
                text.color = new Color(0, 0, 0, i);
                yield return null;
            }
            coOver = true;
            done++;
        }
    }

}
