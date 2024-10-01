using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;
public class CupsAndBallsGame : MonoBehaviour
{
    public GameObject[] cups; 
    public GameObject ball; 
    public float shuffleSpeed = 1f; 
    public float shuffleDuration = 10f; 

    private int ballUnderCupIndex; 
    private bool isShuffling = false;

    public int score = 0;
    public int attempts = 3;

    public XRRayInteractor rayInteractor; 
    public ActionBasedController controller;
    public XRRayInteractor rayInteractor2; 
    public ActionBasedController controller2;

    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI attemptsText;

    private Vector3[] initialCupPositions;  
    private Quaternion[] initialCupRotations;  
    private Vector3 initialBallPosition;

    public GameObject gameOverPanel;
    public GameObject gameStartPanel;
    public TextMeshProUGUI gameOverScoreText;
    public Button resetButton;
    public Button startButton;
    private bool isGameOver = false;

    void Start()
    {
        gameStartPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        UpdateScore();
        updateAttempts();
        
        resetButton.onClick.AddListener(ResetGame);
        startButton.onClick.AddListener(StartGame);

    }

    void StartGame()
    {
        gameStartPanel.SetActive(false);
        initialCupPositions = new Vector3[cups.Length];
        initialCupRotations = new Quaternion[cups.Length];
        for (int i = 0; i < cups.Length; i++)
        {
            initialCupPositions[i] = cups[i].transform.position;
            initialCupRotations[i] = cups[i].transform.rotation;
        }
        initialBallPosition = ball.transform.position;

        ballUnderCupIndex = Random.Range(0, cups.Length);

        StartCoroutine(ShuffleCups());

    }
    IEnumerator ShuffleCups()
    {
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(BallAnimation());
        yield return StartCoroutine(CupTurningAnimation());

        isShuffling = true;
  
        Vector3[] originalPositions = new Vector3[cups.Length];
        for (int i = 0; i < cups.Length; i++)
        {
            originalPositions[i] = cups[i].transform.position;
        }

        float elapsedTime = 0;

        while (elapsedTime < shuffleDuration)
        {
            ShuffleArray(originalPositions);

            yield return StartCoroutine(MoveCupsToPositions(originalPositions));

            elapsedTime += shuffleSpeed;
        }

        isShuffling = false;

    }

    IEnumerator MoveCupsToPositions(Vector3[] targetPositions)
    {

        float t = 0;
        Vector3[] startPositions = new Vector3[cups.Length];

        for (int i = 0; i < cups.Length; i++)
        {
            startPositions[i] = cups[i].transform.position;
        }

        while (t < 1)
        {
            t += Time.deltaTime * shuffleSpeed;
            for (int i = 0; i < cups.Length; i++)
            {
                cups[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }

            ball.transform.position = cups[ballUnderCupIndex].transform.position + new Vector3(0, -0.2f, 0);
            yield return null;

        }
    }

    void ShuffleArray(Vector3[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3 temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
    IEnumerator CupTurningAnimation()
    {
        float liftHeight = 0.5f;
        float liftSpeed = 1f;
        float rotateSpeed = 2f; 
        float dropSpeed = 1f;
        float t = 0;

        Vector3[] startPos = new Vector3[cups.Length];
        Vector3[] liftPos = new Vector3[cups.Length];

        Quaternion[] startRotations = new Quaternion[cups.Length];
        Quaternion[] endRotations = new Quaternion[cups.Length];

        for (int i = 0; i < cups.Length; i++)
        {
            startPos[i] = cups[i].transform.position;
            liftPos[i] = startPos[i] + new Vector3(0, liftHeight, 0);
            startRotations[i] = cups[i].transform.rotation;
            endRotations[i] = startRotations[i] * Quaternion.Euler(0, 180, 0); 
        }

        // Podizanje
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * liftSpeed;
            for (int i = 0; i < cups.Length; i++)
            {
                cups[i].transform.position = Vector3.Lerp(startPos[i], liftPos[i], t); 
            }
            yield return null;
        }

        // Rotacija
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * rotateSpeed;
            for (int i = 0; i < cups.Length; i++)
            {
                cups[i].transform.rotation = Quaternion.Lerp(startRotations[i], endRotations[i], t); 
            }
            yield return null;
        }

        // Spustanje     
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * dropSpeed;
            for (int i = 0; i < cups.Length; i++)
            {
                cups[i].transform.position = Vector3.Lerp(liftPos[i], startPos[i] + new Vector3(0, 0.3f, 0), t); 
            }
            yield return null;
        }
        
    }


    IEnumerator BallAnimation()
    {
        float liftHeight = 1f; 
        float liftSpeed = 1f;  
        float dropSpeed = 1f;  

        // Podizanje loptice
        Vector3 startPos = ball.transform.position;
        Vector3 liftPos = startPos + new Vector3(0, liftHeight, 0);
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * liftSpeed;
            ball.transform.position = Vector3.Lerp(startPos, liftPos, t); // Podizanje 
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // u izabranu casu
        Vector3 targetPos = cups[ballUnderCupIndex].transform.position + new Vector3(0, 0.2f, 0);
        t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * dropSpeed;
            ball.transform.position = Vector3.Lerp(liftPos, targetPos, t); // Spuštanje 
            yield return null;
        }
    }





    private void Update()
    {
        if (gameStartPanel.active == true) {

            if (controller.selectAction.action.triggered || controller2.selectAction.action.triggered)
            {
                RaycastHit hit;
                RaycastHit hit2;
                bool ray1Hit = rayInteractor.TryGetCurrent3DRaycastHit(out hit);
                bool ray2Hit = rayInteractor2.TryGetCurrent3DRaycastHit(out hit2);

                if ((ray1Hit && hit.transform.name == startButton.name) || (ray2Hit && hit2.transform.name == startButton.name))
                {
                    StartGame();
                }
            }

            return; // Block
        }

        if (isGameOver)
        {
            if (controller.selectAction.action.triggered || controller2.selectAction.action.triggered)
            {
                RaycastHit hit;
                RaycastHit hit2;
                bool ray1Hit = rayInteractor.TryGetCurrent3DRaycastHit(out hit);
                bool ray2Hit = rayInteractor2.TryGetCurrent3DRaycastHit(out hit2);

                if ((ray1Hit && hit.transform.name == resetButton.name) || (ray2Hit && hit2.transform.name == resetButton.name))
                {
                    ResetGame();
                }
            }

            return; // Block

        }
        if (!isShuffling) { 
            if (controller.selectAction.action.triggered || controller2.selectAction.action.triggered)
            {
                RaycastHit hit;
                RaycastHit hit2;
                bool ray1Hit = rayInteractor.TryGetCurrent3DRaycastHit(out hit);
                bool ray2Hit = rayInteractor2.TryGetCurrent3DRaycastHit(out hit2);

                if (ray1Hit || ray2Hit)
                {
                    for (int i = 0; i < cups.Length; i++)
                    {
                        if (ray1Hit && hit.transform.name == cups[i].name || ray2Hit && hit2.transform.name == cups[i].name)
                        {
                            Debug.Log("selected cup: " + i);
                            CheckCup(i);
                        }
                    }
                }
            }
        }
    }

    void CheckCup(int selectedCupIndex)
        {
            if (selectedCupIndex == ballUnderCupIndex)
            {
                Debug.Log("Correct");
                score++;
                shuffleSpeed += 0.5f;
                shuffleDuration += 1.5f;
                UpdateScore();
                InitialPositionOfCups();
                StartCoroutine(ShuffleCups());
        }
            else
            {
                Debug.Log("Wrong cup");
                attempts--;
                updateAttempts();
            }
        }

    private void UpdateScore()
    {
        scoreText.text = "SKOR: " + score.ToString();
    }

    private void updateAttempts()
    {
        attemptsText.text = "BR.POKUSAJA: " + attempts.ToString();

        if (attempts == 0)
        {
            GameOver();       
        }
    }

    public void InitialPositionOfCups()
    {
        for (int i = 0; i < cups.Length; i++)
        {
            cups[i].transform.position = initialCupPositions[i];
            cups[i].transform.rotation = initialCupRotations[i];
        }

        ball.transform.position = initialBallPosition;
        //score = 0;
        //attempts = 3;
    }
    void GameOver()
    {
        isGameOver = true;

        gameOverPanel.SetActive(true);

        gameOverScoreText.text = "Konacni skor: " + score.ToString();
    }

    void ResetGame()
    {
        gameOverPanel.SetActive(false);

        score = 0;
        attempts = 3;

        UpdateScore();
        updateAttempts();
        InitialPositionOfCups();
        StartGame();
        isGameOver = false;
    }


}

   