using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamereController : MonoBehaviour
{
    [SerializeField]
    Vector2 center;
    [SerializeField]
    Vector2 mapSize;
    [SerializeField]
    Vector3 cameraPosition;

    public GameObject target; // ī�޶� ���� ���
    public float moveSpeed; // ī�޶� ���� �ӵ�
    private Vector3 targetPosition; // ����� ���� ��ġ
    public GameObject Village;
    public GameObject Deogeon;
    float height;
    float width;

    void Start()
    {
        //Village.gameObject.SetActive(true);//���ӽ��������� ����on
        //Deogeon.gameObject.SetActive(false);//���ӽ��������� ����off
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;


    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (Village.gameObject.activeSelf)
        {
            LimitCameraArea();
        }

        else if (Deogeon.gameObject.activeSelf)
        {
            targetPosition.Set(target.transform.position.x, target.transform.position.y, this.transform.position.z);
            this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }

        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, mapSize * 2);
    }

    void LimitCameraArea()
    {
        targetPosition.Set(target.transform.position.x, target.transform.position.y, this.transform.position.z);
        this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, moveSpeed * Time.deltaTime);
        float lx = mapSize.x - width;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = mapSize.y - height;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

        transform.position = new Vector3(clampX, clampY, -10f);
    }
}
