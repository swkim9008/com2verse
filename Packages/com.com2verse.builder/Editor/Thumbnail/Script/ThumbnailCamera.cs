// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ThumbnailCamera.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-31 오전 11:01
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Com2VerseEditor.Builder
{
    public class ThumbnailCamera
    {
        private const float ConfidenceThreshold = 100000; 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SqrMagnitude(float3 vector) => (float) ((double) vector.x * (double) vector.x + (double) vector.y * (double) vector.y + (double) vector.z * (double) vector.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Max(float a, float b) => a > b ? a : b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Min(float a, float b) => a > b ? b : a;

        public static NativeArray<float3> GetObjectVertices(GameObject targetObject, Allocator allocator = Allocator.Persistent)
        {
            var filters = targetObject.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshRenderers = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            // Calculate total vertices
            int totalVertex = 0;
            foreach (var filter in filters)
            {
                if (Application.isPlaying)
                {
                    totalVertex += filter.mesh.vertexCount;
                }
                else
                {
                    totalVertex += filter.sharedMesh.vertexCount;
                }
            }

            foreach (var renderer in skinnedMeshRenderers)
            {
                totalVertex += renderer.sharedMesh.vertexCount;
            }

            if (totalVertex == 0) return new NativeArray<float3>(0, Allocator.Temp);

            var worldCoordinateVertices = new NativeArray<float3>(totalVertex, allocator, NativeArrayOptions.UninitializedMemory);
            
            // Store world coordinate vertices
            int i = 0;
            foreach (var filter in filters)
            {
                Mesh targetMesh = null;
                if (Application.isPlaying)
                {
                    targetMesh = filter.mesh;
                }
                else
                {
                    targetMesh = filter.sharedMesh;
                }

                var vertices = targetMesh.vertices;
                var localToWorld = filter.transform.localToWorldMatrix;
                foreach (var vertex in vertices)
                {
                    worldCoordinateVertices[i++] = localToWorld.MultiplyPoint3x4(vertex);
                }
            }

            foreach (var renderer in skinnedMeshRenderers)
            {
                var targetMesh = renderer.sharedMesh;

                var vertices = targetMesh.vertices;
                var localToWorld = renderer.transform.localToWorldMatrix;
                foreach (var vertex in vertices)
                {
                    worldCoordinateVertices[i++] = localToWorld.MultiplyPoint3x4(vertex);
                }
            }

            return worldCoordinateVertices;
        }
        
        public static void AlignCamera(Camera camera, GameObject targetObject, int iterationCount = 1, float fitRatio = 1f)
        {
            NativeArray<float3> worldCoordinateVertices = GetObjectVertices(targetObject, Allocator.TempJob);
            if (worldCoordinateVertices.Length != 0)
            {
                AlignCamera(camera, worldCoordinateVertices, iterationCount, fitRatio);
            }
            worldCoordinateVertices.Dispose();
        }

        public static void AlignCamera(Camera camera, NativeArray<float3> worldCoordinateVertices, int iterationCount = 1, float fitRatio = 1f)
        {
            if (worldCoordinateVertices.Length == 0) return;
            
            Profiler.BeginSample("Init Step");
            float3 max = Vector3.negativeInfinity;
            float3 min = Vector3.positiveInfinity;
            foreach (var worldCoordinateVertex in worldCoordinateVertices)
            {
                max.x = Max(max.x, worldCoordinateVertex.x);
                max.y = Max(max.y, worldCoordinateVertex.y);
                max.z = Max(max.z, worldCoordinateVertex.z);
                min.x = Min(min.x, worldCoordinateVertex.x);
                min.y = Min(min.y, worldCoordinateVertex.y);
                min.z = Min(min.z, worldCoordinateVertex.z);
            }

            Bounds boundingBox = new Bounds();
            boundingBox.SetMinMax(min, max);

            float3 worldCenter = boundingBox.center;
            float radius = 0;
            
            foreach (var worldCoordinateVertex in worldCoordinateVertices)
            {
                float dist = SqrMagnitude(worldCoordinateVertex - worldCenter);
                radius = Max(radius, dist);
            }
            radius = (float)System.Math.Sqrt(radius);

            float viewingAngle = Mathf.Atan(fitRatio * Mathf.Tan(camera.fieldOfView / 2 * Mathf.Deg2Rad));
            float cameraDistance = radius / Mathf.Sin(viewingAngle);
            
            // initial guess
            Transform cameraTransform = camera.transform;
            cameraTransform.position = (Vector3)worldCenter - cameraTransform.forward * cameraDistance;
            
            Vector3 initialPosition = cameraTransform.position;
            var clipCoordinateVertices = new NativeArray<float2>(worldCoordinateVertices.Length, Allocator.TempJob);

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            Vector3 up = cameraTransform.up;
            
            Profiler.EndSample();
            
            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                // fit z
                float2 zBound = new float2(-cameraDistance, cameraDistance);
                float currentZ = (zBound.x + zBound.y) / 2;
                
                Profiler.BeginSample("IterationZ");
                for (int i = 0; i < 20; i++)
                {
                    Vector3 targetPosition = initialPosition + forward * currentZ;
                    UnityEngine.Profiling.Profiler.BeginSample("GetMoveDirectionZ");
                    float moveDirection = GetMoveDirectionZ(worldCoordinateVertices, clipCoordinateVertices, camera, targetPosition, cameraTransform.rotation, fitRatio, out float confidence);
                    UnityEngine.Profiling.Profiler.EndSample();

                    if (confidence < 0.0001)
                    {
                        break;
                    }

                    zBound = new float2(moveDirection > 0 ? currentZ : zBound.x, moveDirection > 0 ? zBound.y : currentZ);
                    currentZ = (zBound.x + zBound.y) / 2;
                }
                Profiler.EndSample();

                // fit x/y
                float2 currentWalk = float2.zero;
                float2 localMaxBound = new float2(radius, radius);
                float2 localMinBound = new float2(-radius, -radius);
                
                Profiler.BeginSample("IterationXY");
                for (int i = 0; i < 10; i++)
                {
                    Vector3 targetPosition = initialPosition + forward * currentZ + right * currentWalk.x + up * currentWalk.y;

                    UnityEngine.Profiling.Profiler.BeginSample("GetMoveDirection");
                    float2 moveDirection = GetMoveDirection(worldCoordinateVertices, clipCoordinateVertices, camera, targetPosition, cameraTransform.rotation, out float confidence);
                    UnityEngine.Profiling.Profiler.EndSample();

                    if (confidence < radius / ConfidenceThreshold)
                    {
                        break;
                    }

                    localMaxBound = new float2(moveDirection.x < 0 ? currentWalk.x : localMaxBound.x, moveDirection.y < 0 ? currentWalk.y : localMaxBound.y);
                    localMinBound = new float2(moveDirection.x > 0 ? currentWalk.x : localMinBound.x, moveDirection.y > 0 ? currentWalk.y : localMinBound.y);

                    currentWalk = (localMaxBound + localMinBound) / 2;
                }
                Profiler.EndSample();

                initialPosition = initialPosition + forward * currentZ + right * currentWalk.x + up * currentWalk.y;
            }

            cameraTransform.position = initialPosition;
            clipCoordinateVertices.Dispose();
        }

        private static float2 GetMoveDirection(NativeArray<float3> worldCoordinateVertices , NativeArray<float2> clipCoordinateVertices, Camera camera, Vector3 targetPosition, Quaternion rotation, out float confidence)
        {
            float2 maxNormalizedDeviceCoordinate; float2 minNormalizedDeviceCoordinate;
            (minNormalizedDeviceCoordinate, maxNormalizedDeviceCoordinate) = NDCJob(worldCoordinateVertices, clipCoordinateVertices, camera, targetPosition, rotation);

            confidence = math.lengthsq(maxNormalizedDeviceCoordinate + minNormalizedDeviceCoordinate);

            return new float2(
                maxNormalizedDeviceCoordinate.x > 0 && maxNormalizedDeviceCoordinate.x > math.abs(minNormalizedDeviceCoordinate.x) ? -1 : 1,
                maxNormalizedDeviceCoordinate.y > 0 && maxNormalizedDeviceCoordinate.y > math.abs(minNormalizedDeviceCoordinate.y) ? -1 : 1);
        }

        private static float GetMoveDirectionZ(NativeArray<float3> worldCoordinateVertices, NativeArray<float2> clipCoordinateVertices, Camera camera, Vector3 targetPosition, Quaternion rotation, float targetC, out float confidence)
        {
            float2 maxNormalizedDeviceCoordinate; float2 minNormalizedDeviceCoordinate;
            (minNormalizedDeviceCoordinate, maxNormalizedDeviceCoordinate) = NDCJob(worldCoordinateVertices, clipCoordinateVertices, camera, targetPosition, rotation);

            confidence = math.min(math.abs(targetC * 2 - (maxNormalizedDeviceCoordinate.x - minNormalizedDeviceCoordinate.x)),
               math.abs(targetC * 2 - (maxNormalizedDeviceCoordinate.y - minNormalizedDeviceCoordinate.y)));
            
            if (maxNormalizedDeviceCoordinate.x - minNormalizedDeviceCoordinate.x < targetC * 2 && maxNormalizedDeviceCoordinate.y - minNormalizedDeviceCoordinate.y < targetC * 2)
            {
                return 1;
            }

            return -1;
        }

        private static (float2, float2) NDCJob(NativeArray<float3> worldCoordinateVertices, NativeArray<float2> clipCoordinateVertices, Camera camera, Vector3 targetPosition, Quaternion rotation)
        {
            // fast inverse
            var inverseRotation = Quaternion.Inverse(rotation);
            var t = inverseRotation * -targetPosition;
            var inverseView = Matrix4x4.TRS(t, inverseRotation, Vector3.one);
            var projectionInverse = camera.projectionMatrix * inverseView;

            var job = new CalculateNDCJob()
            {
                Vertices = worldCoordinateVertices,
                ClipCoordinateVertices = clipCoordinateVertices,
                ProjectionInverseView = projectionInverse
            };

            JobHandle jobHandle = job.Schedule(worldCoordinateVertices.Length, 64);
            jobHandle.Complete();

            float2 maxNormalizedDeviceCoordinate = Vector2.negativeInfinity;
            float2 minNormalizedDeviceCoordinate = Vector2.positiveInfinity;

            Profiler.BeginSample("PostProcess1");
            foreach (var clipCoordinateVertex in clipCoordinateVertices)
            {
                maxNormalizedDeviceCoordinate.x = Max(maxNormalizedDeviceCoordinate.x, clipCoordinateVertex.x);
                maxNormalizedDeviceCoordinate.y = Max(maxNormalizedDeviceCoordinate.y, clipCoordinateVertex.y);
                minNormalizedDeviceCoordinate.x = Min(minNormalizedDeviceCoordinate.x, clipCoordinateVertex.x);
                minNormalizedDeviceCoordinate.y = Min(minNormalizedDeviceCoordinate.y, clipCoordinateVertex.y);
            }
            Profiler.EndSample();

            return (minNormalizedDeviceCoordinate, maxNormalizedDeviceCoordinate);
        }

        [BurstCompile]
        private struct CalculateNDCJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float3> Vertices;
            [ReadOnly]
            public Matrix4x4 ProjectionInverseView;

            [WriteOnly]
            public NativeArray<float2> ClipCoordinateVertices;

            public void Execute(int i)
            {
                float3 vector = Vertices[i];
                double w = (double)ProjectionInverseView.m30 * (double)vector.x + (double)ProjectionInverseView.m31 * (double)vector.y + (double)ProjectionInverseView.m32 * (double)vector.z + (double)ProjectionInverseView.m33;
                ClipCoordinateVertices[i] = new float2()
                {
                    x = (float)(((double)ProjectionInverseView.m00 * (double)vector.x + (double)ProjectionInverseView.m01 * (double)vector.y + (double)ProjectionInverseView.m02 * (double)vector.z + (double)ProjectionInverseView.m03) / w),
                    y = (float)(((double)ProjectionInverseView.m10 * (double)vector.x + (double)ProjectionInverseView.m11 * (double)vector.y + (double)ProjectionInverseView.m12 * (double)vector.z + (double)ProjectionInverseView.m13) / w)
                };
            }
        }
    }
}
