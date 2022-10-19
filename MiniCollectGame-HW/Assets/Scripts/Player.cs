using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
public class Player : MonoBehaviour
{
    [Header("Player Move")]
    [SerializeField] FloatingJoystick joystick;
    [SerializeField] float MoveSpeed;
    [SerializeField] float SmoothTurnTime = 0.1f;
    [SerializeField] float CurrentRot;
    [SerializeField] Vector3 Direction;
    Rigidbody rb;
    [Header("Collect Object")]
    [SerializeField] Transform DetectPosition;
    [SerializeField] Transform HoldPosition;
    [SerializeField] float DetectRange = 1f;
    int itemCount=0;
    float itemDistance = 0.5f;
    Collider[] Colliders;
    [SerializeField] LayerMask layer;
    [SerializeField] Transform releasePos;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color=Color.red;
        Gizmos.DrawSphere(DetectPosition.position,DetectRange);
    }




    void Update()
    {
        Colliders = Physics.OverlapSphere(DetectPosition.position, DetectRange,layer);

        
        foreach (var item in Colliders)
        {
            if (item.CompareTag("Collectable"))
            {
                item.tag = "Collected";
                item.transform.parent = HoldPosition;
                var seq = DOTween.Sequence();
                seq.Append(item.transform.DOLocalJump(new Vector3(0, itemCount * itemDistance, 0), 5, 1, 0.5f))
                    .Join(item.transform.DOScale(1.25f,0.1f))
                    .Insert(0.1f,item.transform.DOScale(0.5f,0.2f));
                seq.AppendCallback(() =>
                {
                    item.transform.localRotation = Quaternion.Euler(0, 0, 0);
                });
                itemCount++;
            }
        }
      
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ReleaseArea"))
        {
            foreach (Transform item in HoldPosition)
            {
                var seq = DOTween.Sequence();
                item.transform.parent = releasePos;
                seq.Append(item.transform.DOLocalJump(new Vector3(0, itemCount * itemDistance, 0), 5, 1, 1f))
                    ;
                seq.AppendCallback(() =>
                {
                    item.transform.localRotation = Quaternion.Euler(0, 0, 0);
                });

                itemCount--;

                Debug.Log(item.name);
            }
        }
    }





    private void FixedUpdate()
    {
        float Horizontal = joystick.Horizontal;
        float Vertical = joystick.Vertical;
        Direction = new Vector3(Horizontal, 0, Vertical);
        if (Direction.magnitude>0.01f)
        {
            
            float targetAngle=Mathf.Atan2(Direction.x,Direction.z)*Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref CurrentRot, SmoothTurnTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            rb.MovePosition(transform.position + (Direction * MoveSpeed * Time.deltaTime));
        }
    }
}
