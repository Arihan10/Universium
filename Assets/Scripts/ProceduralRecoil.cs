using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralRecoil : MonoBehaviour
{
    Vector3 currRot, targetRot, targetPos, currPos, initialGunPos;
    Quaternion initialGunRot; 

    [SerializeField] Transform cam;

    [SerializeField] float recoilX, recoilY, recoilZ, kickbackZ, snappiness, returnAmount, camFactor = 1f;

    [SerializeField] bool bothWays = false; 
    
    // Start is called before the first frame update
    void Start()
    {
        initialGunPos = transform.localPosition;
        initialGunRot = transform.localRotation; 
        cam = transform.parent.parent; 
    }

    // Update is called once per frame
    void Update()
    {
        targetRot = Vector3.Lerp(targetRot, Vector3.zero, Time.deltaTime * returnAmount);
        currRot = Vector3.Slerp(currRot, targetRot, Time.deltaTime * snappiness); 
        transform.localRotation = Quaternion.Euler(currRot) * initialGunRot; 
        cam.localRotation = Quaternion.Euler(currRot); 
        Kickback(); 
    }

    public void Recoil() {
        targetPos -= new Vector3(kickbackZ, 0, 0);
        if (!bothWays) targetRot += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ+10f, recoilZ)); 
        else targetRot += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(recoilZ-10f, recoilZ)); 
    }

    void Kickback() {
        targetPos = Vector3.Lerp(targetPos, initialGunPos, Time.deltaTime * returnAmount);
        currPos = Vector3.Lerp(currPos, targetPos, Time.deltaTime * snappiness);
        transform.localPosition = currPos; 
    }
}
