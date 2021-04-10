using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests {
	public class PlaymodeTestScriptExample {

		[Test]
		public void PlaymodeTestScriptExampleSimplePasses () {
		
		}

		[UnityTest]
		public IEnumerator PlaymodeTestScriptExampleWithEnumeratorPasses () {
			yield return null;
		}

	}
}