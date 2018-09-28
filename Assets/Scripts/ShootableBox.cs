using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
public class ShootableBox : MonoBehaviour {

	//The box's current health point total
	//public int currentHealth = 3;
	//public string Color;

	[Header("Numerp de animaciones")]
	public int Rojo;
	public int Azul;
	public int Amarillo;
	public int Verde;
	public int Naranja;
	public int Morado;
	
	[Header("Referencias")]
	[SerializeField]
	private Animation anim;

	public void Damage(string s)
	{
		//Debug.Log("Disparo " + s);
		if(!anim.isPlaying){
			if (s == "Rojo")
			{
				int n = Random.Range(1, Rojo + 1);
				anim.Play("Rojo" + n.ToString());
			}
			else if (s == "Azul")
			{
				int n = Random.Range(1, Azul + 1);
				anim.Play("Azul" + n.ToString());
			}
			else if (s == "Amarillo")
			{
				int n = Random.Range(1, Amarillo + 1);
				anim.Play("Amarillo" + n.ToString());
			}
			else if (s == "Verde")
			{
				int n = Random.Range(1, Verde + 1);
				anim.Play("Verde" + n.ToString());
			}
			else if (s == "Naranja")
			{
				int n = Random.Range(1, Naranja + 1);
				anim.Play("Naranja" + n.ToString());
			}
			else if (s == "Morado")
			{
				int n = Random.Range(1, Morado + 1);
				anim.Play("Morado" + n.ToString());
			}
		}
	}
}
