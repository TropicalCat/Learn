using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenShot : MonoBehaviour 
{
	//用于拍照的相机 
	public Transform m_CameraTrans; 

	// 拍照的地点
	Vector3 m_shotPostion = new Vector3(10000f, 10000f, 0f);

	// Use this for initialization
	void Start () 
	{
	}

	// Update is called once per frame
	void Update () 
	{
	}
		
	public void Capture(GameObject obj)
	{
		if (null == obj) 
		{
			return;
		}

		GameObject gameObject = Instantiate (obj);
		gameObject.transform.localPosition = m_shotPostion;
		gameObject.transform.localRotation = new Quaternion (0, obj.transform.localRotation.y, obj.transform.localRotation.z, obj.transform.localRotation.w);
		AdjustCamera (m_CameraTrans.GetComponent<Camera>(), gameObject);
		CaptureCamera (m_CameraTrans.GetComponent<Camera>(), new Rect(0, 0, 128, 128), obj.name);
		gameObject.SetActive (false);
		Destroy (gameObject);
	}

	private IEnumerator  CaptureByCamera(Camera camera, Rect rect, string fileName)  
	{  
		//等待渲染线程结束  
		yield return new WaitForEndOfFrame();  
		CaptureCamera (camera, rect, fileName);
	}  

	/// <summary>  
	/// 对相机截图。   
	/// </summary>  
	/// <returns>The screenshot2.</returns>  
	/// <param name="camera">Camera.要被截屏的相机</param>  
	/// <param name="rect">Rect.截屏的区域</param>  
	Texture2D CaptureCamera(Camera camera, Rect rect, string fileName)   
	{  
		// 创建一个RenderTexture对象  
		RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);  
		// 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
		camera.targetTexture = rt;  
		camera.Render();  
		//ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像。  
		//ps: camera2.targetTexture = rt;  
		//ps: camera2.Render();  
		//ps: -------------------------------------------------------------------  

		// 激活这个rt, 并从中中读取像素。  
		RenderTexture.active = rt;  
		Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24,false);  
		screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
		screenShot.Apply();  

		// 重置相关参数，以使用camera继续在屏幕上显示  
		camera.targetTexture = null;  
		//ps: camera2.targetTexture = null;  
		RenderTexture.active = null; // JC: added to avoid errors  
		GameObject.Destroy(rt); 

		// 最后将这些纹理数据，成一个png图片文件  
		byte[] bytes = screenShot.EncodeToJPG();
		string filePath = string.Format ("{0}/Resources/ScreenShot/{1}.png", Application.dataPath, fileName);
		filePath = filePath.Replace ('\\','/');
		var dir = filePath.Remove (filePath.LastIndexOf('/'));
		if (!Directory.Exists (dir)) 
		{
			Directory.CreateDirectory (dir);
		}
		System.IO.File.WriteAllBytes(filePath, bytes);  
		Debug.Log(string.Format("截屏了一张照片: {0}", fileName));  

		return screenShot;  
	} 
		
	/// <summary>
	/// 根据obj,调整相机位置，获取一个好的拍照位置
	/// </summary>
	/// <param name="camera">Camera.</param>
	/// <param name="obj">Object.</param>
	void AdjustCamera(Camera camera, GameObject obj)
	{
		if (camera == null || obj == null) 
		{
			return;
		}

		float xMax = 0f;
		float yMax = 0f;
		float zMax = 0f;
		float xMin = 0f;
		float yMin = 0f;
		float zMin = 0f;

		Renderer[] renders = obj.GetComponentsInChildren<Renderer> ();
		if (renders.Length == 0) 
		{
			return;
		}

		xMax = renders [0].bounds.max.x;
		yMax = renders [0].bounds.max.y;
		zMax = renders [0].bounds.max.z;
		xMin = renders [0].bounds.min.x;
		yMin = renders [0].bounds.min.y;
		zMin = renders [0].bounds.min.z;

		foreach(var render in renders)
		{
			Vector3 vMax = render.bounds.max;
			Vector3 vMin = render.bounds.min;

			xMax = Mathf.Max (xMax, vMax.x);
			yMax = Mathf.Max (yMax, vMax.y);
			zMax = Mathf.Max (zMax, vMax.z);
			xMin = Mathf.Min (xMin, vMin.x);
			yMin = Mathf.Min (yMin, vMin.y);
			zMin = Mathf.Min (zMin, vMin.z);
		}

		//盒子大小
		float boxLength = Vector3.Distance(new Vector3(xMax, yMax, zMax), new Vector3(xMin, yMin, zMin));

		float halfFOV = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;

		//摄像机距离盒子中心点的长度
		float cameraDis = boxLength * 0.5f / Mathf.Tan(halfFOV);

		//移动摄像机到物体中心点
		camera.transform.position = new Vector3((xMax + xMin) * 0.5f, (yMax + yMin) * 0.5f, (zMax + zMin) * 0.5f);
		camera.transform.position -= camera.transform.forward * cameraDis;
	}

}
