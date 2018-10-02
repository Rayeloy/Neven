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

	private string animationName;
	
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
				animationName = "Rojo" + n.ToString();
				anim.Play(animationName);
			}
			else if (s == "Azul")
			{
				int n = Random.Range(1, Azul + 1);
				animationName = "Azul" + n.ToString();
				anim.Play(animationName);
			}
			else if (s == "Amarillo")
			{
				int n = Random.Range(1, Amarillo + 1);
				animationName = "Amarillo" + n.ToString();
				anim.Play(animationName);
			}
			else if (s == "Verde")
			{
				int n = Random.Range(1, Verde + 1);
				animationName = "Verde" + n.ToString();
				anim.Play(animationName);
			}
			else if (s == "Naranja")
			{
				int n = Random.Range(1, Naranja + 1);
				animationName = "Naranja" + n.ToString();
				anim.Play(animationName);
			}
			else if (s == "Morado")
			{
				int n = Random.Range(1, Morado + 1);
				animationName = "Morado" + n.ToString();
				anim.Play(animationName);
			}
			anim[animationName].speed = 1.0f;
		}
		else
		{
			anim[animationName].speed = 2.0f;
		}
	}
}
