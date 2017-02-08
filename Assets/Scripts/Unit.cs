using UnityEngine;
using System.Collections;

public class Unit {

	string name;
	int damage;
	int hp;

	public int Damage { get { return damage; } }
	public int HP { get { return hp; } }
	public string Name { get { return name; } }

	public Unit (string name, int damage, int hp) {
		this.name = name;
		this.damage = damage;
		this.hp = hp;
	}
}
