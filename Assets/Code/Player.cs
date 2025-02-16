using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Player : MonoBehaviour
{
    protected Vector3 startPos;
    [HideInInspector] public float startSpeed;
     public float speed;
    [SerializeField] protected float carryingSpeed;
    [SerializeField] protected float rotationSpeed;
    [SerializeField] protected float activeTimer;
    [SerializeField] protected int inactiveTimer;
    [SerializeField] protected Material monochromeMaterial;

    [SerializeField] protected SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] protected GameObject spawnAnimationFeedback;


    [System.Serializable]
    public enum ActivationState {
        Disable,
        Enable
    };

    /// <summary>
    /// define the activation state
    /// </summary>
    public ActivationState activationState;

    protected virtual async void OnEnable ()
    {
        await SpawnAnimation();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        startPos = this.transform.position;
    }

    /// <summary>
    /// to a target 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="speedX"></param>
    protected void MoveForward (Transform target, float speedX)
    {
        // Rotate to face the target
        Vector3 direction = (target.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)); 
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        
        //forward the moves
        Vector3 forward = Vector3.MoveTowards(this.transform.position, target.transform.position, speedX * Time.deltaTime);
        float x = forward.x;
        float y = this.transform.position.y;
        float z = forward.z;
        this.transform.position = new Vector3(x, y, z);

    }

    /// <summary>
    /// to a certain position
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="speedX"></param>
    protected void MoveTowardsPos (Vector3 vec, float speedX)
    {
        // Rotate to face the target
        Vector3 direction = (vec - this.transform.position).normalized;
        if(direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)); 
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        
        //forward the moves
        Vector3 forward = Vector3.MoveTowards(this.transform.position, vec, speedX * Time.deltaTime);
        float x = forward.x;
        float y = this.transform.position.y;
        float z = forward.z;
        this.transform.position = new Vector3(x, y, z);
    }

    /// <summary>
    /// pass object
    /// </summary>
    /// <param name="objectToPass"></param>
    /// <param name="targetPassPos"></param>
    /// <param name="speedX"></param>
    protected void PassObject(GameObject objectToPass, Vector3 targetPassPos, float speedX)
    {
        //forward and pass the ball
        GameCore.gameCore.isBallMove = true;
        Vector3 forward = Vector3.MoveTowards(objectToPass.transform.position, targetPassPos, speedX * Time.deltaTime);
        float x = forward.x;
        float y = objectToPass.transform.position.y;
        float z = forward.z;
        objectToPass.transform.position = new Vector3(x, y, z);
    }

    protected void MoveUp ()
    {
        this.transform.rotation = new Quaternion(0, 0, 0, 0);
        Vector3 move = Vector3.forward * speed * Time.deltaTime;
        this.transform.Translate(move);
    }

    protected virtual void ActivationManager (Material baseMat, Material hair)
    {   
        switch(activationState)
        {
            case ActivationState.Enable:
                Active(baseMat, hair);
                break;
            case ActivationState.Disable:
                Inactive();
                break;
        }
    }

    async UniTask SpawnAnimation ()
    {
        try {
            spawnAnimationFeedback.SetActive(true);
            await UniTask.Delay(1300);
            spawnAnimationFeedback.SetActive(false);
        }
        catch{

        }
    }

    public async void InactiveTime (Material baseMat , Material hairMat)
    {
        await InactiveTimer(baseMat, hairMat);
    }

    async UniTask InactiveTimer (Material baseMat, Material hairMat)
    {
        ActivationManager(baseMat,hairMat);
        await UniTask.Delay(inactiveTimer * 100);
        activationState = ActivationState.Enable;
        ActivationManager(baseMat, hairMat);
    }


    protected void Active (Material baseMaterial , Material hairMaterial)
    {
        try{
            Material[] newMaterials = skinnedMeshRenderer.materials; 
            newMaterials[0] = baseMaterial;
            newMaterials[newMaterials.Length-1] = hairMaterial;
            skinnedMeshRenderer.materials = newMaterials;
        }
        catch {

        }
    }

    protected void Inactive ()
    {
        try {
            Material[] newMaterials = skinnedMeshRenderer.materials; 
            newMaterials[0] = monochromeMaterial;
            newMaterials[newMaterials.Length-1] = monochromeMaterial;
            skinnedMeshRenderer.materials = newMaterials; 
        }
        catch{

        }
    }

    protected virtual void OnTriggerEnter (Collider coll){}
}
