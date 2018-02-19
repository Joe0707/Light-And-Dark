using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }

        set
        {
            instance = value;
        }
    }

    protected virtual void Awake()
    {
        instance = this as T;
    }
    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void OnDestroy()
    {
        instance = null;
    }
}
