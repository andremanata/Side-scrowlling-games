using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawn : MonoBehaviour
{
    public GameObject[] plateforms;
    public GameObject mainCamera;
    private GameObject[] children;

    // Start is called before the first frame update
    void Start()
    {
        children = new GameObject[2];


        children[0] = Instantiate(plateforms[0]) as GameObject;
        children[0].transform.SetParent(gameObject.transform);

        children[1] = Instantiate(plateforms[Random.Range(0, plateforms.Length)]) as GameObject;
        children[1].transform.SetParent(gameObject.transform);
        children[1].transform.position = new Vector3(mainCamera.transform.position.x + 19, 0, 0);
        
    }

    // Update is called once per frame
    void Update()
    {

        if (mainCamera.transform.position.x >= children[1].transform.position.x)
        {
            Destroy(children[0]);
            children[0] = children[1];
            children[1] = Instantiate(plateforms[Random.Range(0, plateforms.Length)]) as GameObject;
            children[1].transform.SetParent(gameObject.transform);
            children[1].transform.position = new Vector3(mainCamera.transform.position.x + 19, 0, 0);
        }

    }

}
