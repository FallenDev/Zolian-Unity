using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class SpawnedNetworkObject : NetworkBehaviour {
        
        /*
        public readonly SyncVar<int> clientSpawnRequestId = new SyncVar<int>();

        public readonly SyncVar<int> serverSpawnRequestId = new SyncVar<int>();
        */

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.SpawnedNetworkObject.OnStartClient()");
        }


        public override void OnStartServer() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.SpawnedNetworkObject.OnStartServer()");
        }
    }
}

