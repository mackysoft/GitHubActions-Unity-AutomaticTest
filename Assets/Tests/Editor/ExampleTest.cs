using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ExampleTest {

	[Test]
	public void Test_1 () {
		bool condition = true;
		Assert.True(condition);
	}

	[UnityTest]
	public IEnumerator UnityTest_1 () {
		yield return new WaitForSeconds(0.5f);
		Assert.Pass();
	}

}