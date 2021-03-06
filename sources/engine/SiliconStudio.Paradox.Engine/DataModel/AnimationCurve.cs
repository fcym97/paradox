// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Internals;
using Quaternion = SiliconStudio.Core.Mathematics.Quaternion;
using Vector3 = SiliconStudio.Core.Mathematics.Vector3;

namespace SiliconStudio.Paradox.DataModel
{
    /// <summary>
    /// Untyped base class for animation curves.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class AnimationCurve
    {
        /// <summary>
        /// Gets or sets the interpolation type.
        /// </summary>
        /// <value>
        /// The interpolation type.
        /// </value>
        public AnimationCurveInterpolationType InterpolationType { get; set; }

        /// <summary>
        /// Gets the type of keyframe values.
        /// </summary>
        /// <value>
        /// The type of keyframe values.
        /// </value>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Gets the size of keyframe values.
        /// </summary>
        /// <value>
        /// The size of keyframe values.
        /// </value>
        public abstract int ElementSize { get; }

        public abstract IReadOnlyList<CompressedTimeSpan> Keys { get; }

        /// <summary>
        /// Writes a new value at the end of the curve (used for building curves).
        /// It should be done in increasing order as it will simply add a new key at the end of <see cref="AnimationCurve{T}.KeyFrames"/>.
        /// </summary>
        /// <param name="newTime">The new time.</param>
        /// <param name="location">The location.</param>
        public abstract void AddValue(CompressedTimeSpan newTime, IntPtr location);

        protected AnimationCurve()
        {
            InterpolationType = AnimationCurveInterpolationType.Linear;
        }
    }

    /// <summary>
    /// Typed class for animation curves.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnimationCurve<T> : AnimationCurve
    {
        /// <summary>
        /// Gets or sets the key frames.
        /// </summary>
        /// <value>
        /// The key frames.
        /// </value>
        public FastList<KeyFrameData<T>> KeyFrames { get; set; }

        /// <inheritdoc/>
        public override Type ElementType
        {
            get { return typeof(T); }
        }

        /// <inheritdoc/>
        public override int ElementSize
        {
            get { return Utilities.UnsafeSizeOf<T>(); }
        }

        /// <inheritdoc/>
        public override IReadOnlyList<CompressedTimeSpan> Keys
        {
            get { return new LambdaReadOnlyCollection<KeyFrameData<T>, CompressedTimeSpan>(KeyFrames, x => x.Time); }
        }

        public AnimationCurve()
        {
            KeyFrames = new FastList<KeyFrameData<T>>();
        }

        /// <summary>
        /// Find key index.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int FindKeyIndex(CompressedTimeSpan time)
        {
            // Simple binary search
            int start = 0;
            int end = KeyFrames.Count - 1;
            while (start <= end)
            {
                int middle = start + ((end - start) >> 1);
                var middleTime = KeyFrames[middle].Time;

                if (middleTime == time)
                {
                    return middle;
                }
                if (middleTime < time)
                {
                    start = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }
            return start;
        }

        /// <inheritdoc/>
        public override void AddValue(CompressedTimeSpan newTime, IntPtr location)
        {
            T value;
            Utilities.UnsafeReadOut(location, out value);
            KeyFrames.Add(new KeyFrameData<T> { Time = (CompressedTimeSpan)newTime, Value = value });
        }
    }
}