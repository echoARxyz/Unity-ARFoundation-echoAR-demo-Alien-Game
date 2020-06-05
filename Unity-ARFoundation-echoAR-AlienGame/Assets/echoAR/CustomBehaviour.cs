/**************************************************************************
* Copyright (C) echoAR, Inc. 2018-2020.                                   *
* echoAR, Inc. proprietary and confidential.                              *
*                                                                         *
* Use subject to the terms of the Terms of Service available at           *
* https://www.echoar.xyz/terms, or another agreement                      *
* between echoAR, Inc. and you, your company or other organization.       *
***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBehaviour : MonoBehaviour
{
    [HideInInspector]
    public Entry entry;

    /// <summary>
    /// EXAMPLE BEHAVIOUR
    /// Queries the database and names the object based on the result.
    /// </summary>

    public GameObject alien;
    public GameObject bullet;
    int alienCount;
    public Vector3 center;
    bool gameOver = false;

    GameObject enemyText;
    TextMesh enemyTextMesh;

    List<GameObject> alienList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        // Add RemoteTransformations script to object and set its entry
        this.gameObject.AddComponent<RemoteTransformations>().entry = entry;

        // Add CameraController script to enable looking (only for desktop)
        if (Camera.main.gameObject.GetComponent<CameraController>() == null) {
          Camera.main.gameObject.AddComponent(typeof(CameraController));
        }

        // Query additional data to get the name
        if (entry.getAdditionalData() != null) {
          string nameString = "";
          if (entry.getAdditionalData().TryGetValue("name", out nameString)) {
            // Set name
            this.gameObject.name = nameString;
          }

          // Get number of aliens
          string alienCountString;
          entry.getAdditionalData().TryGetValue("alienCount", out alienCountString);
          alienCount = int.Parse(alienCountString);
        }

        // Create text to track remaining aliens
        enemyText = new GameObject();
        enemyText.name = "remaining";
        enemyText.transform.position = new Vector3(0, 5, 15);
        enemyTextMesh = enemyText.AddComponent<TextMesh>();
        enemyTextMesh.text = alienCount + " alien(s) remaining";
        enemyTextMesh.fontSize = 15;
        enemyTextMesh.anchor = TextAnchor.MiddleCenter;
        enemyTextMesh.alignment = TextAlignment.Center;
    }

    // Update is called once per frame
    void Update()
    {
      if (gameOver) return;

      if (alien == null) {
        if (this.gameObject.transform.childCount > 0) {
          if (this.gameObject.transform.GetChild(0).gameObject.transform.childCount > 0) {
            alien = this.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
            alien.AddComponent<MeshCollider>();
            alien.transform.position = center + new Vector3(Random.Range(-25, 25), Random.Range(1, 15), Random.Range(-25, 25));
            alienList.Add(alien);

            // dupe alien
            int loop = alienCount - 1;
            while(loop > 0) {
              loop--;
              SpawnAlien();
            }

            // calls function to make aliens move towards the center every 5 seconds
            InvokeRepeating("MoveAliens", 1, 5);
          }
        }
      }
      
      if (Input.GetMouseButtonDown(0)) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
          if (hit.transform != null) {
            // Prevents multiple bullet GameObjects            
            if (bullet != null) {
              Destroy(bullet);
              bullet = null;
            }
            
            // Shoots a bullet at the alien
            Renderer renderer = hit.transform.gameObject.GetComponent<Renderer>();
            bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Rigidbody rigidBody = bullet.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.AddForce(renderer.bounds.center * 300f);

            // Remove alien GameObject and from list
            Destroy(hit.transform.gameObject); // removes the alien
            alienList.Remove(hit.transform.gameObject);
            alienCount--;
            enemyTextMesh.text = alienCount + " alien(s) remaining";

            // Checks if all aliens have been eliminated
            if (alienList.Count == 0) {
              enemyTextMesh.text = "You win!";
              CancelInvoke();
              gameOver = true;
            }
          }
        }
      }
    }

    // Randomly spawn alien objects in the map
    public void SpawnAlien() {
      Vector3 pos = center + new Vector3(Random.Range(-25, 25), Random.Range(1, 15), Random.Range(-25, 25));
      GameObject clone = Instantiate(alien, pos, Quaternion.identity);
      alienList.Add(clone);
    }

    // Move all aliens toward the player
    public void MoveAliens() {
      foreach (GameObject go in alienList) {
        Vector3 origin = new Vector3(0, 0, 0);
        go.transform.position = Vector3.MoveTowards(go.transform.position, origin, 3);

        // Check if an alien has hit the player
        if (go.transform.position.Equals(origin)) {
          enemyTextMesh.text = "Game Over!";
          CancelInvoke();
          gameOver = true;
        }
      }
    }
}