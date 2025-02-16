using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Defender : Player
{
    [SerializeField] protected List<Material> modelMaterialsDefender = new List<Material>();

    [HideInInspector]public bool defenderChase;
    Transform target;
    float distanceToTarget;

    public bool isRun => activationState.Equals(ActivationState.Enable) && defenderChase;
    public bool isCatch => defenderChase && distanceToTarget <= 0.5f;
    public bool isBackToIdle => activationState.Equals(ActivationState.Disable) && !defenderChase;

    [System.Serializable]
    public enum DefenderActionState {
        Standby, 
        None
    };

    public DefenderActionState defenderActionState;

    protected override void OnEnable ()
    {
        base.OnEnable();
        GameCore.gameCore.defenderSpawned.Add(this.gameObject);
    }


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        InactiveTime(modelMaterialsDefender[0], modelMaterialsDefender[1]);
    }

    // Update is called once per frame
    void Update()
    {
        if(activationState.Equals(ActivationState.Enable) && defenderChase && defenderActionState == DefenderActionState.None)
        {
            distanceToTarget = Vector3.Distance(this.transform.position, target.position);
            if(distanceToTarget > 0.5f)   
            {
                defenderActionState = DefenderActionState.None;
                MoveForward(target, speed);
            }
            else
            {
                Attacker attacker = target.GetComponent<Attacker>();
                this.GetComponent<Collider>().enabled = false;
                attacker.GotHit();
                activationState = ActivationState.Disable;
                //catch play animation plays here
            }
        }

        else if(activationState.Equals(ActivationState.Disable) && defenderChase)
        {
            defenderChase = false;
            InactiveTime(modelMaterialsDefender[0], modelMaterialsDefender[1]);       
            defenderActionState = DefenderActionState.Standby;
        }

        else if(activationState.Equals(ActivationState.Enable) && !defenderChase && defenderActionState == DefenderActionState.Standby)
        {
            MoveTowardsPos(startPos, 2.0f);
            defenderActionState = DefenderActionState.None;
            this.GetComponent<Collider>().enabled = true;
        }
    }

    protected override void OnTriggerEnter(Collider coll)
    {
        base.OnTriggerEnter(coll);

        if(coll.transform.tag == "attacker")
        {
            Transform attacker = coll.GetComponent<Transform>();
            if(attacker.childCount >= 7)
            {
                target = coll.GetComponent<Transform>();
                defenderChase = true;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.transform.tag == "attacker")
        {
            Transform attacker = other.GetComponent<Transform>();
            if(attacker.childCount >= 7)
            {
                target = other.GetComponent<Transform>();
                defenderChase = true;
            }
        }
    }
}
