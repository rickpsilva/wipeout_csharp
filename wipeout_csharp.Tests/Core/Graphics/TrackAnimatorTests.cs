using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;
using Xunit;

namespace WipeoutRewrite.Tests.Core.Graphics;

public class TrackAnimatorTests
{
    [Fact]
    public void Update_PickupFacesAnimateColors()
    {
        var animator = new TrackAnimator(LoggerFactory.Create(b => { }).CreateLogger<TrackAnimator>());

        var tri0 = new FT3();
        var tri1 = new FT3();
        var mesh = new Mesh("Track")
        {
            Primitives = new List<Primitive> { tri0, tri1 }
        };

        var face = new TrackLoader.TrackFace
        {
            VertexIndices = new short[] { 0, 1, 2, 3 },
            Flags = 0x02, // pickup
            Color = (10, 20, 30, 40)
        };

        animator.RegisterAnimatedFaces(mesh, new List<TrackLoader.TrackFace> { face });

        animator.Update(0.25f);

        Assert.Equal(tri0.Color, tri1.Color);
        Assert.Equal(255, tri0.Color.a);
        Assert.NotEqual(face.Color, tri0.Color);
    }

    [Fact]
    public void Update_BoostFacesUseBlueTint()
    {
        var animator = new TrackAnimator(LoggerFactory.Create(b => { }).CreateLogger<TrackAnimator>());

        var tri0 = new FT3();
        var tri1 = new FT3();
        var mesh = new Mesh("Track")
        {
            Primitives = new List<Primitive> { tri0, tri1 }
        };

        var face = new TrackLoader.TrackFace
        {
            VertexIndices = new short[] { 0, 1, 2, 3 },
            Flags = 0x20, // boost flag
            Color = (1, 2, 3, 4)
        };

        animator.RegisterAnimatedFaces(mesh, new List<TrackLoader.TrackFace> { face });

        animator.Update(0.1f);

        Assert.Equal((byte)0, tri0.Color.r);
        Assert.Equal((byte)0, tri0.Color.g);
        Assert.Equal((byte)255, tri0.Color.b);
        Assert.Equal((byte)255, tri0.Color.a);
        Assert.Equal(tri0.Color, tri1.Color);
    }
}