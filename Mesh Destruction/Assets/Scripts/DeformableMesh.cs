using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableMesh : MonoBehaviour
{
	public float malleability = 0.05f;
	public float radius = 0.1f;
	public int maxHits = 10;
	public bool enableDebris = false;
	public GameObject debrisBase;
	public int material = 0;	

	private Mesh mesh;
	private MeshCollider meshCollider;
	private Shader shader;
	private Vector3[] verticies;
	private int hits = 0;

	private void Start()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		meshCollider = GetComponent<MeshCollider>();
		shader = GetComponent<Shader>();
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

				//Deform vertices
				verticies = mesh.vertices;
				float scale;
				for (int i = 0; i < verticies.Length; i++)
				{
					//Get deformation scale based on distance
					scale = Mathf.Clamp(radius - (point3 - verticies[i]).magnitude, 0, radius);

					//Deform by impulse multiplied by scale and strength parameter
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
					GameObject debris = Instantiate(debrisBase, collision.transform.position, collision.transform.rotation) as GameObject;

					switch(material)
                    {
						case 0:
							debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Wood;
							debris.GetComponent<MeshDestroy>().CutCascades = 5;
							break;
						case 1:
							debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Brick;
							debris.GetComponent<MeshDestroy>().CutCascades = 8;
							break;
						case 2:
							debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Tile;
							debris.GetComponent<MeshDestroy>().CutCascades = 11;
							break;
						case 3:
							debris.GetComponent<MeshDestroy>().debrisMat = MeshDestroy.Material.Metal;
							break;
					}
					debris.GetComponent<Renderer>().material.shader = shader;
					debris.GetComponent<MeshDestroy>().DestroyMesh();

					Destroy(debris, 2);
                }
			}
        }
	}
}