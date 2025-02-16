using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameCore : MonoBehaviour
{
    [Header("Attributes")]
    public int gameTimerDuration;
    public int matchRounds; 

    [Header("Game Objects")]
    public GameObject ballPrefab;
    public GameObject attackerPrefab;
    public GameObject defenderPrefab;
    public GameObject defenderGoal;
    public GameObject mazeGenerator;
    public AttackerSensor attackerSensor;
    public DefenderSensor defenderSensor;


    [HideInInspector] public GameObject ball;


    [Header("Object Loop")]
    [HideInInspector]public List<GameObject>attackers = new List<GameObject>();
    [HideInInspector]public List<GameObject>defenders = new List<GameObject>();
    [HideInInspector]public List<GameObject>attackerSpawned = new List<GameObject>();
    [HideInInspector]public List<GameObject>defenderSpawned = new List<GameObject>();
    public int attackersFirstSpawn;
    public int defenderFirstSpawn;


    [Header("UI Text")]
    public TextMeshProUGUI durationTime;
    public TextMeshProUGUI announcementText;
    public TextMeshProUGUI roundMatchText;


    [Header("Logic")]
    [HideInInspector] public bool hitBallAttacker; 
    [HideInInspector] public bool resetRound;
    [HideInInspector] public bool isBallMove;


    [Header("Scores Attributes")]
    public TextMeshProUGUI attackerScoreText;
    public TextMeshProUGUI defenderScoreText;
    public int attackerScore;
    public int defenderScore;
    public Score scoreAttacker;
    public Score scoreDefender;
    [HideInInspector] public int energyCost;



    [HideInInspector] public Timer timer;
    [HideInInspector] public Ball ballX;

    [System.Serializable]
    public struct EnergyBarProperties 
    {
        public UnityEngine.UI.Image attackerEnergyContent;
        public UnityEngine.UI.Image defenderEnergyContent;
        public TextMeshProUGUI attackerEnergyText;
        public TextMeshProUGUI defenderEnergyText;
        public float energyMax;
        public float energyAttacker;
        public float energyDefender;
        public int attackerPoint;
        public int defenderPoint;
    }

    public EnergyBarProperties energyBarProperties;

    [HideInInspector]public BarPoint barPointAttacker;
    [HideInInspector]public BarPoint barPointDefender;

    
    /// <summary>
    /// ball random spawn attribute
    /// </summary>
    [System.Serializable]
    public struct BallRandomSpawnAttribute {
        public float xRandomMinSpawn;
        public float xRandomMaxSpawn;
        public float yRandomMinSpawn;
        public float yRandomMaxSpawn;
    }

    public BallRandomSpawnAttribute ballRandomSpawnAttribute;

    public static GameCore gameCore;

    void Awake ()
    {
        gameCore = this;
    }

    void OnEnable()
    {
        resetRound = false;
    } 

    // Start is called before the first frame update
    void Start()
    {
         //spawn the ball
        ball = Instantiate(ballPrefab);
        ballX = new Ball(ballRandomSpawnAttribute.xRandomMinSpawn, ballRandomSpawnAttribute.xRandomMaxSpawn,ballRandomSpawnAttribute.yRandomMinSpawn, ballRandomSpawnAttribute.yRandomMaxSpawn, ball);
        ballX.SetPosition();

        //start spawn rules
        StartAttackerSpawn();
        StartDefenderSpawn();

        //spawn regenerative timer
        barPointAttacker = new BarPoint(energyBarProperties.attackerEnergyContent, energyBarProperties.attackerEnergyText, 
            energyBarProperties.energyAttacker, energyBarProperties.energyMax, energyBarProperties.attackerPoint);
        barPointDefender = new BarPoint(energyBarProperties.defenderEnergyContent, energyBarProperties.defenderEnergyText,
            energyBarProperties.energyDefender, energyBarProperties.energyMax, energyBarProperties.defenderPoint);
        barPointAttacker.SetContentValue(energyBarProperties.attackerPoint);
        barPointDefender.SetContentValue(energyBarProperties.defenderPoint);
        barPointAttacker.RegenerationPoint(true);
        barPointDefender.RegenerationPoint(false);

         //create scores instance
        scoreAttacker = new Score(attackerScoreText, attackerScore);
        scoreDefender = new Score(defenderScoreText, defenderScore);
        attackerScoreText.text = attackerScore.ToString();
        defenderScoreText.text = defenderScore.ToString();

        //create instance timer
        timer = new Timer(durationTime, gameTimerDuration);
        timer.DecreaseTimer("a");
    }

    // Update is called once per frame
    void Update()
    {
        ResetGame();
    }

    void StartAttackerSpawn ()
    {
        for(int i = 0; i < attackersFirstSpawn; i++)
        {
            GameObject attackerClone = Instantiate(attackerPrefab);
            attackers.Add(attackerClone);
            attackerClone.SetActive(false);
        }
    }

    void StartDefenderSpawn()
    {
        for(int i = 0; i < defenderFirstSpawn; i++)
        {
            GameObject defenderClone = Instantiate(defenderPrefab);
            defenders.Add(defenderClone);
            defenderClone.SetActive(false);
        }
    }

    /// <summary>
    /// debugging method to reset the game
    /// </summary>
    void ResetGame()
    {
        if(Input.GetKeyDown(KeyCode.Return))SceneManager.LoadScene(0);
        if(resetRound) ResetRoundStart();
    }

    /// <summary>
    /// reset current round
    /// </summary>
    public async void ResetRoundStart ()
    {
        await ResetRound();
    }

    async UniTask ResetRound ()
    {
        attackerSensor.gameObject.SetActive(false);
        defenderSensor.gameObject.SetActive(false);
        await UniTask.Delay(2000);
        attackerSensor.gameObject.SetActive(true);
        defenderSensor.gameObject.SetActive(true);
        if(ball.transform.parent != null)
        {
            Attacker attacker = ball.transform.parent.GetComponent<Attacker>();
            if(!attacker.isDie || !attacker.isCatch)
                return;
        }
        ball.transform.SetParent(null);
        ball.GetComponent<Collider>().enabled = true;
        if(!isBallMove)ballX.ResetBallPosition();
        scoreDefender.ResetUpdateScore();
        scoreAttacker.ResetUpdateScore();
        GameCore.gameCore.hitBallAttacker = false;
        timer.RoundIncrement("a");
        timer.Reset();
        resetRound = false;
    }

}


/// <summary>
/// ball reference
/// </summary>
public class Ball
{
    float xMin;
    float xMax;
    float yMin;
    float yMax;
    Vector3 startPosition;
    GameObject ballPrefab;

    public Ball (float xPosMin , float xPosMax , float yPosMin, float yPosMax, GameObject prefab)
    {
        xMin = xPosMin;
        xMax = xPosMax;
        yMin = yPosMin;
        yMax = yPosMax;
        ballPrefab = prefab;
    }

    public GameObject SetPosition ()
    {
        float xPos = UnityEngine.Random.Range(xMin, xMax);
        float yPos = ballPrefab.transform.position.y;
        float zPos = UnityEngine.Random.Range(yMin, yMax);
        ballPrefab.transform.position = new Vector3(xPos, yPos, zPos);
        startPosition = ballPrefab.transform.position;
        return ballPrefab;
    }

    public void ResetBallPosition ()
    {
        ballPrefab.transform.position = startPosition;
        ballPrefab.GetComponent<Collider>().enabled = true;
    }
}

/// <summary>
/// base derived of the Timer class
/// </summary>
public class Timer {
    
    TextMeshProUGUI textTimer;
    int startTimerValue;

    public Timer (TextMeshProUGUI textTimer, int startTimerValue)
    {
        this.textTimer = textTimer;
        this.startTimerValue = startTimerValue;
    }

    public async void DecreaseTimer (string turnID)
    {
        while(startTimerValue > 0) await DecreaseProcess(turnID);
    }

    async UniTask DecreaseProcess (string turnID)
    {
        await UniTask.Delay(1000);
        if(startTimerValue <= 1)
        {
            //win lose here
            if(GameCore.gameCore.matchRounds < 5) 
            {
                GameCore.gameCore.matchRounds++;
                GameCore.gameCore.roundMatchText.text = "Match " + GameCore.gameCore.matchRounds.ToString();
                startTimerValue = GameCore.gameCore.gameTimerDuration;
            }
            else 
            {
                Announcement announcement = new Announcement(GameCore.gameCore.announcementText, turnID);
                announcement.WinAnnouncement(GameCore.gameCore.attackerScoreText.text, 
                    GameCore.gameCore.defenderScoreText.text);
            }

        }   
        startTimerValue -= 1;
        textTimer.text = startTimerValue.ToString() + "s";
    }

    public int GetMatchTimeRunOut ()
    {
        return startTimerValue;
    }

    public void RoundIncrement (string turnID)
    {
        if(GameCore.gameCore.matchRounds < 5) 
        { 
            GameCore.gameCore.matchRounds++;
            GameCore.gameCore.roundMatchText.text = "Match " + GameCore.gameCore.matchRounds.ToString();
        }
        else 
        {
            Announcement announcement = new Announcement(GameCore.gameCore.announcementText, turnID);
            announcement.WinAnnouncement(GameCore.gameCore.attackerScoreText.text, GameCore.gameCore.defenderScoreText.text);
        }
    }

    public void Reset ()
    {
        startTimerValue = 140;
        textTimer.text = startTimerValue.ToString() + "s";
    }
}

public class Score {
    TextMeshProUGUI scoreTextMesh;
    int scoreValue;
    bool scoreUpdate;

    public Score (TextMeshProUGUI scoreText, int scoreValue)
    {
        scoreTextMesh = scoreText;
        this.scoreValue = scoreValue;
    }

    public void IncreaseScore (GameObject attackerObj = null)
    {
        if(scoreUpdate)return;
        scoreUpdate = true;
        scoreValue++;
        scoreTextMesh.text = scoreValue.ToString();
        if(attackerObj != null)
        {
            GameCore.gameCore.attackers.Add(attackerObj);
            attackerObj.SetActive(false);
        }
        GameCore.gameCore.ResetRoundStart();
    }

    public int GetScoreValue ()
    {
        return scoreValue;
    }

    public void ResetUpdateScore()
    {
        scoreUpdate = false;
    }
}

public class BarPoint {
    UnityEngine.UI.Image contentBar;
    TextMeshProUGUI pointText;
    float pointValue;
    float maxPointValue;
    int currentPointValue;

    public BarPoint (UnityEngine.UI.Image contentBarX, TextMeshProUGUI pointTextX,  float pointValueX, float maxPointValueX, int currentPoint)
    {
        contentBar = contentBarX;
        pointValue = pointValueX;
        pointText = pointTextX;
        maxPointValue = maxPointValueX;
        currentPointValue = currentPoint;
    }

    public void SetContentValue (int point)
    {
        contentBar.fillAmount = pointValue/maxPointValue;
        pointText.text = point.ToString();
    }

    public async void RegenerationPoint (bool isAttacker)
    {
       while(true)await RegenerativePointProccess(isAttacker);
    }

    async UniTask RegenerativePointProccess (bool isAttacker)
    {
        await UniTask.Delay(1500);
        if(pointValue >= 1.0f && currentPointValue < 6)
        {
            pointValue = 0;
            if(isAttacker && GameCore.gameCore.energyBarProperties.attackerPoint < 6)
            {
                GameCore.gameCore.energyBarProperties.attackerPoint++;
                pointText.text = GameCore.gameCore.energyBarProperties.attackerPoint.ToString();
                SetContentValue(GameCore.gameCore.energyBarProperties.attackerPoint);
            }
            else if(GameCore.gameCore.energyBarProperties.defenderPoint < 6)
            {
                GameCore.gameCore.energyBarProperties.defenderPoint++;
                pointText.text = GameCore.gameCore.energyBarProperties.defenderPoint.ToString();
                SetContentValue(GameCore.gameCore.energyBarProperties.defenderPoint);
            }
        }
        pointValue += 0.5f;
        if(isAttacker && GameCore.gameCore.energyBarProperties.attackerPoint < 6)
        {
            pointText.text = GameCore.gameCore.energyBarProperties.attackerPoint.ToString();
            SetContentValue(GameCore.gameCore.energyBarProperties.attackerPoint);
        }
        else if(!isAttacker && GameCore.gameCore.energyBarProperties.defenderPoint < 6)
        {
            pointText.text = GameCore.gameCore.energyBarProperties.defenderPoint.ToString();
            SetContentValue(GameCore.gameCore.energyBarProperties.defenderPoint);
        }    
    }

    public int GetCurrentPointValue()
    {
        return currentPointValue;
    }
}

public class Announcement {

    TextMeshProUGUI announcementText;
    string attackerPlayerId;

    public Announcement (TextMeshProUGUI announcementTextX, string attackerPlayerIdX)
    {
        announcementText = announcementTextX;
        attackerPlayerId = attackerPlayerIdX;
    }

    public void WinAnnouncement (string score1, string score2)
    {
        int score1Int = int.Parse(score1);
        int score2Int = int.Parse(score2);

        if(score1Int > score2Int) 
        {
            switch(attackerPlayerId)
            {
                case "a":
                    announcementText.text = "Player 1 Win";
                    break;
                case "b":
                    announcementText.text = "Player 2 Win";
                    break;
            }
            AnimateAnnouncementText();
        }
        else if(score1Int < score2Int)
        {
            switch(attackerPlayerId)
            {
                case "a":
                    announcementText.text = "Player 2 Win";
                    break;
                case "b":
                    announcementText.text = "Player 1 Win";
                    break;
            }
            AnimateAnnouncementText();
        }
        else if(score1Int == score2Int)
        {
            announcementText.text = "Draw";
            AnimateAnnouncementText();
        }
    }

    void AnimateAnnouncementText ()
    {
        LeanTween.scale(announcementText.gameObject, Vector3.one, 1.5f)
            .setIgnoreTimeScale(true)
            .setOnComplete (() => {
                //restart the match 
                WaitBeforeReset();
            });
    }

    void WaitBeforeReset ()
    {
        if(announcementText.text == "Draw")
        {
            GameCore.gameCore.mazeGenerator.SetActive(true);
            GameCore.gameCore.attackerSensor.enabled = false;
            GameCore.gameCore.defenderSensor.enabled = false;
            GameCore.gameCore.gameObject.SetActive(false);
            for(int i = 0; i < GameCore.gameCore.attackerSpawned.Count; i++)GameCore.gameCore.attackerSpawned[i].SetActive(false);
            for(int x = 0; x < GameCore.gameCore.defenderSpawned.Count; x++)GameCore.gameCore.defenderSpawned[x].SetActive(false);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
