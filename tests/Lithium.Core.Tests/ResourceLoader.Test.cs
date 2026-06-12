using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Lithium.Core.Tests;

/// <summary>
/// Tests for Lithium.Core.ResourceLoader.cs
/// </summary>
public class ResourceLoaderTests {
	/// <summary>
	/// Tests the LoadResourceStream throws an exception and the stream is null with an invalid input.
	/// </summary>
	[Fact]
	public void LoadResourceStreamTest01() {
		Stream? stream = null;
		Exception ex = Assert.Throws<ArgumentException>(
			() => stream = ResourceLoader.LoadResourceStream(typeof(ResourceLoaderTests).Assembly, "")
		);

		Assert.NotNull(ex);
		Assert.Null(stream);
	}
	/// <summary>
	/// Tests that LoadResourceStream correctly gets the stream for a valid resource.
	/// </summary>
	[Fact]
	public void LoadResourceStreamTest02() {
		Stream? stream = ResourceLoader.LoadResourceStream(typeof(ResourceLoaderTests).Assembly, "Lithium.__resources__.text-resource.txt");
		Assert.NotNull(stream);
	}

	/// <summary>
	/// Tests that FetchResources correctly returns all resources when no extension is given.
	/// </summary>
	[Fact]
	public void FetchresourcesTest01() {
		IEnumerable<string> resources = ResourceLoader.FetchResources(typeof(ResourceLoaderTests).Assembly);

		Assert.Collection(resources,
			s => s.Equals("xml-resource.xml"),
			s => s.Equals("empty-xml-resource.xml"),
			s => s.Equals("text-resource.txt")
		);
	}
	/// <summary>
	/// Tests that FetchResources only returns resources matching the supplied extension.
	/// </summary>
	[Fact]
	public void FetchresourcesTest02() {
		IEnumerable<string> resources = ResourceLoader.FetchResources(typeof(ResourceLoaderTests).Assembly, ".xml");

		Assert.Collection(resources,
			s => s.Equals("xml-resource.xml"),
			s => s.Equals("empty-xml-resource.xml")
		);
	}
}
