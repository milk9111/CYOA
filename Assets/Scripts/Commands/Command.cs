using System;
using UnityEngine;

namespace Commands
{
    public abstract class Command : MonoBehaviour
    {
        public virtual void Execute(string[] args)
        {
           throw new NotImplementedException(); 
        }
    }
}