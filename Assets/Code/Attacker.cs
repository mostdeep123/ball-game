using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Attacker : Player
{
    public List<Material> modelMaterialsAttacker = new List<Material>();
    [SerializeField] Attacker nearestAttacker;
    [SerializeField] Collider passSensor;
    [SerializeField] GameObject footArrows;
    float distanceToGoal;
    
    [HideInInspector] public bool catched;
    [HideInInspector] public bool hitFench;
    Transform ballTrans;
    public bool isRun => activationState.Equals(ActivationState.Enable);
    public bool isDie => speed <= 0 && hitFench && this.transform.childCount < 7;
    public bool isIdle => distanceToGoal <= 0.3f && this.transform.childCount >= 7;
    public bool backToIdle => !catched;
    public bool isCatch => catched;
    bool isBallNeedPass => ballTrans != null && ballTrans.transform.parent == null;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        this.GetComponent<Collider>().enabled = true;
        passSensor.enabled = false;
        footArrows.SetActive(false);
        GameCore.gameCore.attackerSpawned.Add(this.gameObject);
    }

    void OnDisable ()
    {
        hitFench = false;
        GameCore.gameCore.attackerSpawned.Remove(this.gameObject);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        startSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameCore.gameCore.ball != null && activationState.Equals(ActivationState.Enable)) 
            MoveToBall(GameCore.gameCore.ball.transform, GameCore.gameCore.defenderGoal.transform);
        if(isBallNeedPass && nearestAttacker != null)
        {
            if (nearestAttacker.isDie || !nearestAttacker.gameObject.activeSelf)
            {
                nearestAttacker.gameObject.GetComponent<Attacker>().nearestAttacker = null;
                nearestAttacker = null;
                //defender got score
                GameCore.gameCore.scoreDefender.IncreaseScore(this.gameObject);
                return;
            }
        }
        if (isBallNeedPass && nearestAttacker != null && !nearestAttacker.isDie && nearestAttacker.gameObject.activeSelf)
        {
            PassObject(ballTrans.gameObject, nearestAttacker.transform.position, 3.3f);
            if (nearestAttacker.isDie || !nearestAttacker.gameObject.activeSelf)
            {
                nearestAttacker.gameObject.GetComponent<Attacker>().nearestAttacker = null;
                nearestAttacker = null;
                //defender got score
                GameCore.gameCore.scoreDefender.IncreaseScore(this.gameObject);
                return;
            }
            // Check distance only when the ball is moving towards the next player
            if (ballTrans != null && Vector3.Distance(ballTrans.position, nearestAttacker.transform.position) <= 0.5f)
            {
                GameCore.gameCore.isBallMove = false;
                SetBallParent(ballTrans, nearestAttacker.transform);
                nearestAttacker.passSensor.enabled = true;
                nearestAttacker.ballTrans = ballTrans;
                ballTrans = null;
                GameCore.gameCore.attackers.Add(this.gameObject);
                this.gameObject.SetActive(false);
            }
        }
    }

    void MoveToBall(Transform target, Transform defenderGoal)
    {
        if(!GameCore.gameCore.hitBallAttacker)
        {
            MoveForward(target, speed);
        }
        else 
        {
            if(this.transform.childCount >= 7)
            {
                distanceToGoal = Vector3.Distance(this.transform.position, defenderGoal.transform.position);
                if(distanceToGoal > 0.3f)
                {
                    footArrows.SetActive(true);
                    MoveForward(defenderGoal, carryingSpeed);
                }
                else
                {
                    activationState = ActivationState.Disable;
                    GameCore.gameCore.ballX.ResetBallPosition();
                    if(ballTrans != null)ballTrans.transform.parent = null;          //unparent the ball
                    GameCore.gameCore.attackers.Add(this.gameObject);
                    this.gameObject.SetActive(false);
                    
                    //attacker get the score
                    GameCore.gameCore.scoreAttacker.IncreaseScore();
                }
            }
            else
            {
                MoveUp();
            }
        }
    }

    public async void GotHit()
    {
        catched = true;
        footArrows.SetActive(false);
        this.GetComponent<Collider>().enabled = false;
        activationState = ActivationState.Disable;
        passSensor.enabled = false;
        speed = 0;
        Inactive();
        if(ballTrans != null)ballTrans.transform.parent = null;          //unparent the ball
        await WaitDisable();
    }

    async UniTask WaitDisable ()
    {
        //check if ball is in this attacker
        if(this.transform.childCount >= 7)
        {
            if(ballTrans != null)ballTrans.transform.parent = null;          //unparent the ball
        }
        if(nearestAttacker == null)
        {
            await UniTask.Delay(4500);
            GameCore.gameCore.scoreDefender.IncreaseScore();
            nearestAttacker = null;
            GameCore.gameCore.attackers.Add(this.gameObject);
            this.gameObject.SetActive(false);
        }
        else
        {
            float distanceBall = Vector3.Distance(ballTrans.gameObject.transform.position, nearestAttacker.transform.position);
            if(nearestAttacker != null)
            {
                if(nearestAttacker.isDie || nearestAttacker.isCatch)
                {
                    GameCore.gameCore.scoreDefender.IncreaseScore();
                    nearestAttacker = null;
                    GameCore.gameCore.attackers.Add(this.gameObject);
                    this.gameObject.SetActive(false);

                    return;
                }
                
                await UniTask.WaitUntil(()=> distanceBall <= 0.5f);
                GameCore.gameCore.scoreDefender.IncreaseScore();
                GameCore.gameCore.isBallMove = false;
                nearestAttacker = null;
                GameCore.gameCore.attackers.Add(this.gameObject);
                this.gameObject.SetActive(false);
            }
            GameCore.gameCore.scoreDefender.IncreaseScore();
            nearestAttacker = null;
            GameCore.gameCore.attackers.Add(this.gameObject);
            this.gameObject.SetActive(false);
        }
    }

    protected override void ActivationManager(Material baseMat, Material hair)
    {
        base.ActivationManager(baseMat, hair);
    }

    async void PlayDieAnimation ()
    {
        await PlayDieAnimationProcess();
    }

    async UniTask PlayDieAnimationProcess ()
    {
        try {
            await UniTask.Delay(1500);
            GameCore.gameCore.attackers.Add(this.gameObject);
            this.gameObject.SetActive(false);
        }
        catch{

        }
    }

    void SetBallParent (Transform ballTrans, Transform target)
    {
        GameCore.gameCore.hitBallAttacker = true;
        ballTrans.transform.SetParent(target);
        float x = 0;
        float y = -0.2f;
        float z = 1.0f;
        ballTrans.transform.localPosition = new Vector3(x, y, z);
    }

    protected override void OnTriggerEnter(Collider coll)
    {
        base.OnTriggerEnter(coll);

        if(coll.transform.tag == "ball")
        {
            coll.enabled = false;
            passSensor.enabled = true;
            ballTrans= coll.GetComponent<Transform>();
            if(!isDie && !isCatch)SetBallParent(ballTrans, this.transform);
        }

        if(coll.transform.tag == "fence")
        {
            if(this.transform.childCount >= 7) return;
            speed = 0;
            hitFench = true;
            this.GetComponent<Collider>().enabled = true;
            //play die animation
            
            PlayDieAnimation();
        }

        if(coll.transform.tag == "attacker")
        {
            if(this.transform.childCount >= 7)
                nearestAttacker  = coll.GetComponent<Attacker>();
        }
    }

    void OnTriggerStay (Collider coll)
    {
        if(coll.transform.tag == "attacker")
        {
            if(this.transform.childCount >= 7)
                nearestAttacker  = coll.GetComponent<Attacker>();
        }
    }

    void OTriggerExit(Collider other)
    {
        if(other.transform.tag == "attacker")
        {
            if(this.transform.childCount >= 7)
                nearestAttacker = null;
        }
    }
}


