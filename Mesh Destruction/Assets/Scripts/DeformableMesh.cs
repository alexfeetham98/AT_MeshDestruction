using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class DeformableMesh : MonoBehaviour
{
	[Range(0.0f,3)]
	public float malleability = 0.05f;
	[Range(0.0f,0.5f)]
	public float radius = 0.1f;
	[Range(0,15)]
	public int maxHits = 10;
	[Range(1,10)]
	public int cutCascades = 5;
	public bool enableDebris = false;
	


	//Debris Components
	private GameObject debrisBase;
	private Mesh mesh;
	private MeshCollider meshCollider;
	private MeshRenderer meshRenderer;
	//public int material = 0;	

	private Vector3[] verticies;
	private int hits = 0;

	private void Start()
	{
		debrisBase = GameObject.Find("DebrisBase");
		mesh = GetComponent<MeshFilter>().mesh;
		meshCollider = GetComponent<MeshCollider>();
		meshRenderer = GetComponent<MeshRenderer>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Projectile" || collision.gameObject.tag == "Tool" )
		{
			if (hits < maxHits)
			{
				//Get point, normal, and impulse
				Vector3 point3 = transform.InverseTransformPoint(collision.GetContact(0).point);
				Vector3 normal = transform.InverseTransformDirection(collision.GetContact(0).normal);
				float impluse = collision.impulse.magnitude;

				verticies = mesh.vertices;
				float scale;
				for (int i = 0; i < verticies.Length; i++)
				{
					//Get deformation scale based on distance
					scale = Mathf.Clamp(radius - (point3 - verticies[i]).magnitude, 0, radius);

					//Deform the verticies based on the impluse, scale and malleability
					verticies[i] += normal * impluse * scale * malleability;
				}

				//Apply changes to collider and mesh
				mesh.vertices = verticies;
				meshCollider.sharedMesh = mesh;

				//Recalculate mesh normal and bounds - Unity functions
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();

				hits = hits + 1;

				if (collision.gameObject.tag == "Projectile")
				{
					Destroy(collision.gameObject);
				}
				
				if (enableDebris)
                {
					//Create new game object
					GameObject debris = Instantiate(debrisBase, collision.transform.position, collision.transform.rotation) as GameObject;

                    #region Old Instantiation
                    //switch(material)
                    //{
                    //	case 0:
                    //		debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Wood;
                    //		debris.GetComponent<MeshDestroy>().CutCascades = 8;
                    //		break;
                    //	case 1:
                    //		debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Brick;
                    //		debris.GetComponent<MeshDestroy>().CutCascades = 10;
                    //		break;
                    //	case 2:
                    //		debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Tile;

                    //		break;
                    //	case 3:
                    //		debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Metal;
                    //		break;
                    //}
                    #endregion

                    debris.GetComponent<MeshDestroy>().CutCascades = cutCascades;					//Set the amount of slices for the debris chunk
					debris.GetComponent<MeshRenderer>().material = meshRenderer.material;			//Set the material to match that of the object that was hit
					debris.transform.localScale = new Vector3(radius * 2, radius * 2, radius);		//Set the scale of the debris object to match that of the hole created by the deform
                    debris.GetComponent<MeshDestroy>().DestroyMesh();								//Slice the debris

                    #region Threading

					///Unity does not not allow you to edit anything to do with
					///a game object in child threads.
					///The intention would have been to calcute the mesh slicing
					///on a child thread to reduce fps lag however its just not allowed.

                    //Thread destroyThread = new Thread(debris.GetComponent<MeshDestroy>().DestroyMesh);
                    //lock (debris)
                    //{
                    //    destroyThread.Start();
                    //}
                    //while (destroyThread.IsAlive) { Debug.Log("Thread Running"); }
                    #endregion

                }
            }
        }
	}
}