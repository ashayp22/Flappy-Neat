using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pipe : MonoBehaviour
{

    public GameObject upPipe;
    public GameObject downPipe;
    public GameObject upSupport;
    public GameObject downSupport;

    public float topBottom;
    public float bottomTop;


    // Start is called before the first frame update
    void Start()
    {

        //decides pos of upPipe
        float downPos = Random.Range(-1.8f, 0.7f);
        float upPos = downPos + 2f;

        topBottom = upPos;
        bottomTop = downPos;

        upPipe.transform.position = new Vector2(0, upPos);

        //sets down pipe
        downPipe.transform.position = new Vector2(0, downPos);

        //now does the supports for top and bottom

        downSupport.transform.position = new Vector2(0, (-2.5f + downPos - 0.1f) / 2f);
        upSupport.transform.position = new Vector2(0, (upPos + 3.05f + 0.1f) / 2f);

        downSupport.transform.localScale = new Vector2(0.2f, Mathf.Abs(-2.5f - (downPos - 0.1f)) * 0.09f);
        upSupport.transform.localScale = new Vector2(0.2f, Mathf.Abs((upPos + 0.1f) - 3.05f) * 0.09f);

        transform.position = new Vector2(1.2f, 0);

    }

    // Update is called once per frame
    void Update()
    {
        //move the pipe
        transform.position = new Vector2(transform.position.x - 0.01f, transform.position.y);
    }
}
