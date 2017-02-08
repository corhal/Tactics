using UnityEngine;
using System.Collections;

public class Squad : MonoBehaviour {

	Unit unitType;
	int unitCount;
	int totalHp;
	int totalDamage;

	public void TakeDamage (int amount) {
		totalHp -= amount;
		RefreshStats ();
	}

	public void DealDamage (Squad enemy) {
		enemy.TakeDamage (totalDamage);
	}

	public void InitializeSquad () {

		RefreshStats ();
	}

	void RefreshStats () {
		float fracUnitCount = totalHp / unitType.HP;
		unitCount = Mathf.CeilToInt (fracUnitCount);
		totalDamage = unitType.Damage * unitCount;

		if (unitCount <= 0) {
			Destroy (gameObject);
		}
	}
}
