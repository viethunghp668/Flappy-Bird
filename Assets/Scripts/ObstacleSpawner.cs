using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour {

	[SerializeField] private float waitTime, difficultRange = 2f, easyRange = 0.5f;
	[SerializeField] private GameObject obstaclePrefab;
	private float tempTime, previousPipeHeight;
	private int difficultyCounter = -1;
	private float[] difficultyLevelArr = {0, 0, 0, 1, 1};
	// 0  is easy, 1 is hard

	void Start(){
		tempTime = waitTime - Time.deltaTime;
		difficultyCounter = -1;
		previousPipeHeight = transform.position.y;
	}

	void LateUpdate () {
		if(GameManager.Instance.GameState()){
			tempTime += Time.deltaTime;
			if(tempTime > waitTime){
				// Wait for some time, create an obstacle, then set wait time to 0 and start again
				tempTime = 0;
				Vector3 pipePos = transform.position;
				previousPipeHeight = pipePos.y = CalculatePipeHeight();
				GameObject pipeClone = Instantiate(obstaclePrefab, pipePos, transform.rotation);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D col){
		if(col.gameObject.transform.parent != null){
			Destroy(col.gameObject.transform.parent.gameObject);
		}else{
			Destroy(col.gameObject);
		}
	}

	float CalculatePipeHeight() { 
		difficultyCounter++;

		if(difficultyCounter >= difficultyLevelArr.Length || difficultyCounter < 0)
			difficultyCounter = 0;

		float previousHeightDirection = previousPipeHeight / Mathf.Abs(previousPipeHeight);
		float currentDifficultyLevel = difficultyLevelArr[difficultyCounter];

		float heightOffset = UnityEngine.Random.Range(0 + (easyRange * currentDifficultyLevel), easyRange + (difficultRange * currentDifficultyLevel));
		float newHeight = previousPipeHeight + (heightOffset * -previousHeightDirection);

		return newHeight;
	}

}
