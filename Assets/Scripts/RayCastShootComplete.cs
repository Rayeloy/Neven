using UnityEngine;
using System.Collections;

public class RayCastShootComplete : MonoBehaviour {

	public float fireRate = 0.25f;										// Number in seconds which controls how often the player can fire
	public float weaponRange = 50f;										// Distance in Unity units over which the player can fire
	public Transform gunEnd;											// Holds a reference to the gun end object, marking the muzzle location of the gun
	public Color[] listaDeColores;
	private int ColorActivo = 0;

	[Header("Referencias")]
	public Camera fpsCam;												// Holds a reference to the first person camera
	private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);	// WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
	public AudioSource gunAudio;										// Reference to the audio source which will play our shooting sound effect
	public LineRenderer laserLine;										// Reference to the LineRenderer component which will display our laserline
	private float nextFire;												// Float to store the time the player will be allowed to fire again, after firing

	void Update () 
	{
		// Check if the player has pressed the fire button and if enough time has elapsed since they last fired
		if (Input.GetButtonDown("Fire1") && Time.time > nextFire) 
		{
			string colorUsar = numToString(ColorActivo);
			// Update the time when our player can fire next
			nextFire = Time.time + fireRate;

			// Start our ShotEffect coroutine to turn our laser line on and off
            StartCoroutine (ShotEffect());

            // Create a vector at the center of our camera's viewport
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));

			// Set the start position for our visual effect for our laser to the position of gunEnd
			laserLine.SetPosition (0, gunEnd.position);

			RaycastHit[] hits;
        	hits = Physics.RaycastAll(rayOrigin, fpsCam.transform.forward, weaponRange);

			if(hits.Length > 0)
			{
				//ordenar el array
				RaycastHit temp;
				for (int write = 0; write < hits.Length; write++)
				{
					for (int sort = 0; sort < hits.Length - 1; sort++)
					{
						if (hits[sort].distance > hits[sort + 1].distance)
						{
							temp = hits[sort + 1];
							hits[sort + 1] = hits[sort];
							hits[sort] = temp;
						}       
					} 
				}

				 for (int i = 0; i < hits.Length; i++)
				{
					RaycastHit hit = hits[i];
					ShootableBox sB = hit.collider.GetComponent<ShootableBox>();

					if (sB)
					{
						Debug.Log(colorUsar);
						laserLine.SetPosition (1, hit.point);

						sB.Damage (colorUsar);
						return;
					}

					DonutDeColor dC = hit.collider.GetComponent<DonutDeColor>();
					if (dC)
					{
						if ((colorUsar == "Amarillo" && dC.Color == "Azul") || (colorUsar == "Azul" && dC.Color == "Amarillo")){
							colorUsar = "Verde";
							Debug.Log(colorUsar);
						}
						else if ((colorUsar == "Rojo" && dC.Color == "Azul") || (colorUsar == "Azul" && dC.Color == "Rojo")){
							colorUsar = "Morado";
						}
						else if ((colorUsar == "Amarillo" && dC.Color == "Rojo") || (colorUsar == "Rojo" && dC.Color == "Amarillo")){
							colorUsar = "Naranja";
						}
						laserLine.SetPosition (1, hit.point);
					}
					else
					{
						laserLine.SetPosition (1, hit.point);
					}
				}
			}
			else
			{
				laserLine.SetPosition (1, rayOrigin + (fpsCam.transform.forward * weaponRange));
			}
		}
		else if(Input.GetButtonDown("Fire2")){
			ColorActivo = (ColorActivo + 1)%3;
		}
	}

	private string numToString (int i){
		if (i == 0){
			return "Rojo";
		}
		else if (i == 1)
			return "Azul";
		else{
			return "Amarillo";
		}
	}

	private IEnumerator ShotEffect()
	{
		// Play the shooting sound effect
		//gunAudio.Play ();

		laserLine.startColor = listaDeColores[ColorActivo];
		laserLine.endColor = listaDeColores[ColorActivo];
		// Turn on our line renderer
		laserLine.enabled = true;

		//Wait for .07 seconds
		yield return shotDuration;

		// Deactivate our line renderer after waiting
		laserLine.enabled = false;
	}
}