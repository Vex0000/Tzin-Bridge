using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class CameraFollow : MonoBehaviour
{

    public Player player;

    public Transform target;

    public Vector3 offset;

    public Vector3 localStartRotation;

    public float lerpSpeed;

    public bool isDecoupled;

    public Vector3 StartPos;


    public float mousePanMovementSens = .05f;

    public float maxDistance = 100f;

    public float mouseZoomSens;

    public Vector2 minMaxCamFOV;

    public Camera main;

    public float horzRotSpeed;
    public float vertRotSpeed;

    public Vector2 minMaxVertRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        main = GetComponent<Camera>();
        localStartRotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

        
        // Panning
        Vector2 pan = new Vector2();
        if (Input.GetKey(KeyCode.A))
        {
            pan.x -= 1;
        } 
        
        if (Input.GetKey(KeyCode.D))
        {
            pan.x += 1;
        } 
        
        if (Input.GetKey(KeyCode.W))
        {
            pan.y += 1;
        } 
        
        if (Input.GetKey(KeyCode.S))
        {
            pan.y -= 1;
        } 
          
        if (pan.sqrMagnitude!=0)
        {
            isDecoupled = true;
            
            if (Vector3.Distance(target.transform.position, transform.position + new Vector3(pan.x, 0, pan.y) * (Time.deltaTime * mousePanMovementSens)) < maxDistance)
            {
                var transform1 = transform;
                float py = transform1.position.y;
                
                transform1.position += ((transform1.forward * pan.y) + (transform1.right * pan.x)) * Time.deltaTime * mousePanMovementSens;
                
                transform1.position -= Vector3.up * (transform1.position.y -py);

                //transform.position += new Vector3(pan.x, 0, pan.y) * (Time.deltaTime * mousePanMovementSens);
            }

        }

        // Prevents the camera from moving with a blocking action
        if (GameAction.prefomringBlockingAction == null)
        {
            // Rotation
            Vector2 rot = new Vector2();
            if (Input.GetKey(KeyCode.E))
            {
                rot.x += 1;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                rot.x -= 1;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                rot.y -= 1;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                rot.y += 1;
            }

            // Horz rotation
            transform.Rotate(Vector3.up, rot.x * Time.deltaTime * horzRotSpeed, Space.World);

            // Vert rotation

            if (!((transform.rotation.eulerAngles.x < minMaxVertRotation.x && rot.y < 0) || (transform.rotation.eulerAngles.x > minMaxVertRotation.y && rot.y > 0)))
            {
                transform.Rotate(Vector3.right, rot.y * Time.deltaTime * vertRotSpeed, Space.Self);
            }

        }
        // Zooming
        
        if (Input.mouseScrollDelta.y != 0)
        {
            main.fieldOfView = Mathf.Clamp(main.fieldOfView + -Input.mouseScrollDelta.y*Time.deltaTime*mouseZoomSens, minMaxCamFOV.x, minMaxCamFOV.y);
        }
        
      
        
    }
    
    public void ResetToTarget()
    {
        if (Vector3.Distance(transform.position, target.position+offset) >1 && !isDecoupled)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(localStartRotation), lerpSpeed );
            Invoke(nameof(ResetToTarget), Time.deltaTime);
        }
    }

    public void StartResetToTarget()
    {
        isDecoupled = false;
        Invoke(nameof(ResetToTarget), Time.deltaTime);
    }
}
