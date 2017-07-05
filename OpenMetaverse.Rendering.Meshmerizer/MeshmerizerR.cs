/* Copyright (c) 2008 Robert Adams
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * The name of the copyright holder may not be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/*
 * Portions of this code are:
 * Copyright (c) Contributors, http://idealistviewer.org
 * The basic logic of the extrusion code is based on the Idealist viewer code.
 * The Idealist viewer is licensed under the three clause BSD license.
 */
/*
 * MeshmerizerR class implments OpenMetaverse.Rendering.IRendering interface
 * using PrimMesher (http://forge.opensimulator.org/projects/primmesher).
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Text;
using OMV = OpenMetaverse;
using OpenMetaverse.StructuredData;
using OMVR = OpenMetaverse.Rendering;

namespace OpenMetaverse.Rendering
{
    /// <summary>
    /// Meshing code based on the Idealist Viewer (20081213).
    /// </summary>
    [RendererName("MeshmerizerR")]
    public class MeshmerizerR : OMVR.IRendering
    {
        /// <summary>
        /// Generates a basic mesh structure from a primitive
        /// A 'SimpleMesh' is just the prim's overall shape with no material information.
        /// </summary>
        /// <param name="prim">Primitive to generate the mesh from</param>
        /// <param name="lod">Level of detail to generate the mesh at</param>
        /// <returns>The generated mesh or null on failure</returns>
        public OMVR.SimpleMesh GenerateSimpleMesh(OMV.Primitive prim, OMVR.DetailLevel lod)
        {
            PrimMesher.PrimMesh newPrim = GeneratePrimMesh(prim, lod, false);
            if (newPrim == null)
                return null;

            SimpleMesh mesh = new SimpleMesh();
            mesh.Path = new Path();
            mesh.Prim = prim;
            mesh.Profile = new Profile();
            mesh.Vertices = new List<Vertex>(newPrim.coords.Count);
            for (int i = 0; i < newPrim.coords.Count; i++)
            {
                PrimMesher.Coord c = newPrim.coords[i];
                mesh.Vertices.Add(new Vertex { Position = new Vector3(c.X, c.Y, c.Z) });
            }

            mesh.Indices = new List<ushort>(newPrim.faces.Count * 3);
            for (int i = 0; i < newPrim.faces.Count; i++)
            {
                PrimMesher.Face face = newPrim.faces[i];
                mesh.Indices.Add((ushort)face.v1);
                mesh.Indices.Add((ushort)face.v2);
                mesh.Indices.Add((ushort)face.v3);
            }

            return mesh;
        }

        /// <summary>
        /// Generates a basic mesh structure from a sculpted primitive.
        /// 'SimpleMesh's have a single mesh and no faces or material information.
        /// </summary>
        /// <param name="prim">Sculpted primitive to generate the mesh from</param>
        /// <param name="sculptTexture">Sculpt texture</param>
        /// <param name="lod">Level of detail to generate the mesh at</param>
        /// <returns>The generated mesh or null on failure</returns>
        public OMVR.SimpleMesh GenerateSimpleSculptMesh(OMV.Primitive prim, System.Drawing.Bitmap sculptTexture, OMVR.DetailLevel lod)
        {
            OMVR.FacetedMesh faceted = GenerateFacetedSculptMesh(prim, sculptTexture, lod);

            if (faceted != null && faceted.Faces.Count == 1)
            {
                Face face = faceted.Faces[0];

                SimpleMesh mesh = new SimpleMesh();
                mesh.Indices = face.Indices;
                mesh.Vertices = face.Vertices;
                mesh.Path = faceted.Path;
                mesh.Prim = prim;
                mesh.Profile = faceted.Profile;
                mesh.Vertices = face.Vertices;

                return mesh;
            }

            return null;
        }

        /// <summary>
        /// Create a faceted mesh from prim shape parameters.
        /// Generates a a series of faces, each face containing a mesh and
        /// material metadata.
        /// A prim will turn into multiple faces with each being independent
        /// meshes and each having different material information.
        /// </summary>
        /// <param name="prim">Primitive to generate the mesh from</param>
        /// <param name="lod">Level of detail to generate the mesh at</param>
        /// <returns>The generated mesh</returns >
        public OMVR.FacetedMesh GenerateFacetedMesh(OMV.Primitive prim, OMVR.DetailLevel lod)
        {
            bool isSphere = ((OMV.ProfileCurve)(prim.PrimData.profileCurve & 0x07) == OMV.ProfileCurve.HalfCircle);
            PrimMesher.PrimMesh newPrim = GeneratePrimMesh(prim, lod, true);
            if (newPrim == null)
                return null;

            // copy the vertex information into OMVR.IRendering structures
            OMVR.FacetedMesh omvrmesh = new OMVR.FacetedMesh();
            omvrmesh.Faces = new List<OMVR.Face>();
            omvrmesh.Prim = prim;
            omvrmesh.Profile = new OMVR.Profile();
            omvrmesh.Profile.Faces = new List<OMVR.ProfileFace>();
            omvrmesh.Profile.Positions = new List<OMV.Vector3>();
            omvrmesh.Path = new OMVR.Path();
            omvrmesh.Path.Points = new List<OMVR.PathPoint>();
            var indexer = newPrim.GetVertexIndexer();

            for (int i = 0; i < indexer.numPrimFaces; i++)
            {
                OMVR.Face oface = new OMVR.Face();
                oface.Vertices = new List<OMVR.Vertex>();
                oface.Indices = new List<ushort>();
                oface.TextureFace = prim.Textures.GetFace((uint)i);

                for (int j = 0; j < indexer.viewerVertices[i].Count; j++)
                {
                    var vert = new OMVR.Vertex();
                    var m = indexer.viewerVertices[i][j];
                    vert.Position = new Vector3(m.v.X, m.v.Y, m.v.Z);
                    vert.Normal = new Vector3(m.n.X, m.n.Y, m.n.Z);
                    vert.TexCoord = new OMV.Vector2(m.uv.U, 1.0f - m.uv.V);
                    oface.Vertices.Add(vert);
                }

                for (int j = 0; j < indexer.viewerPolygons[i].Count; j++)
                {
                    var p = indexer.viewerPolygons[i][j];
                    // Skip "degenerate faces" where the same vertex appears twice in the same tri
                    if (p.v1 == p.v2 || p.v1 == p.v2 || p.v2 == p.v3) continue;
                    oface.Indices.Add((ushort)p.v1);
                    oface.Indices.Add((ushort)p.v2);
                    oface.Indices.Add((ushort)p.v3);
                }

                omvrmesh.Faces.Add(oface);
            }

            return omvrmesh;
        }

        /// <summary>
        /// Create a sculpty faceted mesh. The actual scuplt texture is fetched and passed to this
        /// routine since all the context for finding teh texture is elsewhere.
        /// </summary>
        /// <returns>The faceted mesh or null if can't do it</returns>
        public OMVR.FacetedMesh GenerateFacetedSculptMesh(OMV.Primitive prim, System.Drawing.Bitmap scupltTexture, OMVR.DetailLevel lod)
        {
            PrimMesher.SculptMesh.SculptType smSculptType;
            switch (prim.Sculpt.Type)
            {
                case OpenMetaverse.SculptType.Cylinder:
                    smSculptType = PrimMesher.SculptMesh.SculptType.cylinder;
                    break;
                case OpenMetaverse.SculptType.Plane:
                    smSculptType = PrimMesher.SculptMesh.SculptType.plane;
                    break;
                case OpenMetaverse.SculptType.Sphere:
                    smSculptType = PrimMesher.SculptMesh.SculptType.sphere;
                    break;
                case OpenMetaverse.SculptType.Torus:
                    smSculptType = PrimMesher.SculptMesh.SculptType.torus;
                    break;
                default:
                    smSculptType = PrimMesher.SculptMesh.SculptType.plane;
                    break;
            }
            // The lod for sculpties is the resolution of the texture passed.
            // The first guess is 1:1 then lower resolutions after that
            // int mesherLod = (int)Math.Sqrt(scupltTexture.Width * scupltTexture.Height);
            int mesherLod = 32; // number used in Idealist viewer
            switch (lod)
            {
                case OMVR.DetailLevel.Highest:
                    break;
                case OMVR.DetailLevel.High:
                    break;
                case OMVR.DetailLevel.Medium:
                    mesherLod /= 2;
                    break;
                case OMVR.DetailLevel.Low:
                    mesherLod /= 4;
                    break;
            }
            PrimMesher.SculptMesh newMesh =
                new PrimMesher.SculptMesh(scupltTexture, smSculptType, mesherLod, true, prim.Sculpt.Mirror, prim.Sculpt.Invert);

            int numPrimFaces = 1;       // a scuplty has only one face

            // copy the vertex information into OMVR.IRendering structures
            OMVR.FacetedMesh omvrmesh = new OMVR.FacetedMesh();
            omvrmesh.Faces = new List<OMVR.Face>();
            omvrmesh.Prim = prim;
            omvrmesh.Profile = new OMVR.Profile();
            omvrmesh.Profile.Faces = new List<OMVR.ProfileFace>();
            omvrmesh.Profile.Positions = new List<OMV.Vector3>();
            omvrmesh.Path = new OMVR.Path();
            omvrmesh.Path.Points = new List<OMVR.PathPoint>();

            Dictionary<OMVR.Vertex, int> vertexAccount = new Dictionary<OMVR.Vertex, int>();


            for (int ii = 0; ii < numPrimFaces; ii++)
            {
                vertexAccount.Clear();
                OMVR.Face oface = new OMVR.Face();
                oface.Vertices = new List<OMVR.Vertex>();
                oface.Indices = new List<ushort>();
                oface.TextureFace = prim.Textures.GetFace((uint)ii);
                int faceVertices = newMesh.coords.Count;
                OMVR.Vertex vert;

                for (int j = 0; j < faceVertices; j++)
                {
                    vert = new OMVR.Vertex();
                    vert.Position = new Vector3(newMesh.coords[j].X, newMesh.coords[j].Y, newMesh.coords[j].Z);
                    vert.Normal = new Vector3(newMesh.normals[j].X, newMesh.normals[j].Y, newMesh.normals[j].Z);
                    vert.TexCoord = new Vector2(newMesh.uvs[j].U, newMesh.uvs[j].V);
                    oface.Vertices.Add(vert);
                }

                for (int j = 0; j < newMesh.faces.Count; j++)
                {
                    oface.Indices.Add((ushort)newMesh.faces[j].v1);
                    oface.Indices.Add((ushort)newMesh.faces[j].v2);
                    oface.Indices.Add((ushort)newMesh.faces[j].v3);
                }

                if (faceVertices > 0)
                {
                    oface.TextureFace = prim.Textures.FaceTextures[ii];
                    if (oface.TextureFace == null)
                    {
                        oface.TextureFace = prim.Textures.DefaultTexture;
                    }
                    oface.ID = ii;
                    omvrmesh.Faces.Add(oface);
                }
            }

            return omvrmesh;
        }

        /// <summary>
        /// Apply texture coordinate modifications from a
        /// <seealso cref="TextureEntryFace"/> to a list of vertices
        /// </summary>
        /// <param name="vertices">Vertex list to modify texture coordinates for</param>
        /// <param name="center">Center-point of the face</param>
        /// <param name="teFace">Face texture parameters</param>
        public void TransformTexCoords(List<OMVR.Vertex> vertices, OMV.Vector3 center, OMV.Primitive.TextureEntryFace teFace, Vector3 primScale)
        {
            // compute trig stuff up front
            float cosineAngle = (float)Math.Cos(teFace.Rotation);
            float sinAngle = (float)Math.Sin(teFace.Rotation);

            for (int ii = 0; ii < vertices.Count; ii++)
            {
                // tex coord comes to us as a number between zero and one
                // transform about the center of the texture
                OMVR.Vertex vert = vertices[ii];

                // aply planar tranforms to the UV first if applicable
                if (teFace.TexMapType == MappingType.Planar)
                {
                    Vector3 binormal;
                    float d = Vector3.Dot(vert.Normal, Vector3.UnitX);
                    if (d >= 0.5f || d <= -0.5f)
                    {
                        binormal = Vector3.UnitY;
                        if (vert.Normal.X < 0f) binormal *= -1;
                    }
                    else
                    {
                        binormal = Vector3.UnitX;
                        if (vert.Normal.Y > 0f) binormal *= -1;
                    }
                    Vector3 tangent = binormal % vert.Normal;
                    Vector3 scaledPos = vert.Position * primScale;
                    vert.TexCoord.X = 1f + (Vector3.Dot(binormal, scaledPos) * 2f - 0.5f);
                    vert.TexCoord.Y = -(Vector3.Dot(tangent, scaledPos) * 2f - 0.5f);
                }
                
                float repeatU = teFace.RepeatU;
                float repeatV = teFace.RepeatV;
                float tX = vert.TexCoord.X - 0.5f;
                float tY = vert.TexCoord.Y - 0.5f;

                vert.TexCoord.X = (tX * cosineAngle + tY * sinAngle) * repeatU + teFace.OffsetU + 0.5f;
                vert.TexCoord.Y = (-tX * sinAngle + tY * cosineAngle) * repeatV + teFace.OffsetV + 0.5f;
                vertices[ii] = vert;
            }
            return;
        }

        // The mesh reader code is organized so it can be used in several different ways:
        //
        // 1. Fetch the highest detail displayable mesh as a FacetedMesh:
        //      var facetedMesh = GenerateFacetedMeshMesh(prim, meshData);
        // 2. Get the header, examine the submeshes available, and extract the part
        //              desired (good if getting a different LOD of mesh):
        //      OSDMap meshParts = UnpackMesh(meshData);
        //      if (meshParts.ContainsKey("medium_lod"))
        //          var facetedMesh = MeshSubMeshAsFacetedMesh(prim, meshParts["medium_lod"]):
        // 3. Get a simple mesh from one of the submeshes (good if just getting a physics version):
        //      OSDMap meshParts = UnpackMesh(meshData);
        //      OMV.Mesh flatMesh = MeshSubMeshAsSimpleMesh(prim, meshParts["physics_mesh"]);
        //
        // "physics_convex" is specially formatted so there is another routine to unpack
        //              that section:
        //      OSDMap meshParts = UnpackMesh(meshData);
        //      if (meshParts.ContainsKey("physics_convex"))
        //          OSMap hullPieces = MeshSubMeshAsConvexHulls(prim, meshParts["physics_convex"]):
        //
        // LL mesh format detailed at http://wiki.secondlife.com/wiki/Mesh/Mesh_Asset_Format

        /// <summary>
        /// Create a mesh faceted mesh from the compressed mesh data.
        /// This returns the highest LOD renderable version of the mesh.
        ///
        /// The actual mesh data is fetched and passed to this
        /// routine since all the context for finding the data is elsewhere.
        /// </summary>
        /// <returns>The faceted mesh or null if can't do it</returns>
        public OMVR.FacetedMesh GenerateFacetedMeshMesh(OMV.Primitive prim, byte[] meshData)
        {
            OMVR.FacetedMesh ret = null;
            OSDMap meshParts = UnpackMesh(meshData);
            if (meshParts != null)
            {
                byte[] meshBytes = null;
                foreach (string partName in ["high_lod", "medium_lod", "low_lod", "lowest_lod"]) {
                    if (meshParts.ContainsKey(partName))
                    {
                        meshBytes = meshParts[partName];
                        break;
                    }
                }
                if (meshBytes != null)
                {
                    ret = MeshSubMeshAsFacetedMesh(prim, meshBytes);
                }

            }
            return ret;
        }

        public OMVR.FacetedMesh MeshSubMeshAsFacetedMesh(OMV.Primitive prim, byte[] compressedMeshData)
        {
            OMVR.FacetedMesh ret = null;
            OSD meshOSD = Helpers.ZDecompressOSD(compressedMeshData);

            if (meshOSD == null || !(meshOSD is OSDArray))
            {
                return null;
            }
            OSDArray meshFaces = (OSDArray)meshOSD;
           
            for (int faceNum = 0;


                OSDArray decodedMeshOsdArray = (OSDArray)facesOSD;

                for (int faceNr = 0; faceNr < decodedMeshOsdArray.Count; faceNr++)

            return ret;
        }

        public OMVR.FacetedMesh SomeFunctionHoldingOldCode(OMV.Primitive prim, byte[] meshData)
        {
            coords = new List<Coord>();
            faces = new List<Face>();
            OSD meshOsd = null;

            mConvexHulls = null;
            mBoundingHull = null;

            long start = 0;
            using (MemoryStream data = new MemoryStream(meshData)) {
                try {
                    OSD osd = OSDParser.DeserializeLLSDBinary(data);
                    if (osd is OSDMap)
                        meshOsd = (OSDMap)osd;
                    else {
                        // m_log.Warn("[Mesh}: unable to cast mesh asset to OSDMap");
                        return null;
                    }
                }
                catch (Exception e) {
                    // m_log.Error("[MESH]: Exception deserializing mesh asset header:" + e.ToString());
                    throw (new Exception("Exception deserializing mesh asset header: " + e.ToString()));
                }
                start = data.Position;
            }

            if (meshOsd is OSDMap) {
                OSDMap meshParams = null;
                OSDMap map = (OSDMap)meshOsd;
                if (map.ContainsKey("high_lod")) {
                    meshParams = (OSDMap)map["high_lod"]; // if all else fails, use highest LOD display mesh and hope it works :)
                }
                else if (map.ContainsKey("medium_lod")) {
                    meshParams = (OSDMap)map["medium_lod"]; // if no physics mesh, try to fall back to medium LOD display mesh
                }
                // else if (map.ContainsKey("physics_shape")) {
                //     physicsParms = (OSDMap)map["physics_shape"]; // old asset format
                // }
                // else if (map.ContainsKey("physics_mesh")) {
                //     physicsParms = (OSDMap)map["physics_mesh"]; // new asset format
                // }

                /*
                if (map.ContainsKey("physics_convex")) { // pull this out also in case physics engine can use it
                    OSD convexBlockOsd = null;
                    try {
                        OSDMap convexBlock = (OSDMap)map["physics_convex"];
                        {
                            int convexOffset = convexBlock["offset"].AsInteger() + (int)start;
                            int convexSize = convexBlock["size"].AsInteger();

                            byte[] convexBytes = new byte[convexSize];

                            System.Buffer.BlockCopy(primShape.SculptData, convexOffset, convexBytes, 0, convexSize);

                            try {
                                convexBlockOsd = DecompressOsd(convexBytes);
                            }
                            catch (Exception e) {
                                m_log.ErrorFormat("{0} prim='{1}': exception decoding convex block: {2}", LogHeader, primName, e);
                                //return false;
                            }
                        }

                        if (convexBlockOsd != null && convexBlockOsd is OSDMap) {
                            convexBlock = convexBlockOsd as OSDMap;

                            if (debugDetail) {
                                string keys = LogHeader + " keys found in convexBlock: ";
                                foreach (KeyValuePair<string, OSD> kvp in convexBlock)
                                    keys += "'" + kvp.Key + "' ";
                                m_log.Debug(keys);
                            }

                            Vector3 min = new Vector3(-0.5f, -0.5f, -0.5f);
                            if (convexBlock.ContainsKey("Min")) min = convexBlock["Min"].AsVector3();
                            Vector3 max = new Vector3(0.5f, 0.5f, 0.5f);
                            if (convexBlock.ContainsKey("Max")) max = convexBlock["Max"].AsVector3();

                            List<Vector3> boundingHull = null;

                            if (convexBlock.ContainsKey("BoundingVerts")) {
                                byte[] boundingVertsBytes = convexBlock["BoundingVerts"].AsBinary();
                                boundingHull = new List<Vector3>();
                                for (int i = 0; i < boundingVertsBytes.Length;) {
                                    ushort uX = Utils.BytesToUInt16(boundingVertsBytes, i); i += 2;
                                    ushort uY = Utils.BytesToUInt16(boundingVertsBytes, i); i += 2;
                                    ushort uZ = Utils.BytesToUInt16(boundingVertsBytes, i); i += 2;

                                    Vector3 pos = new Vector3(
                                        Utils.UInt16ToFloat(uX, min.X, max.X),
                                        Utils.UInt16ToFloat(uY, min.Y, max.Y),
                                        Utils.UInt16ToFloat(uZ, min.Z, max.Z)
                                    );

                                    boundingHull.Add(pos);
                                }

                                mBoundingHull = boundingHull;
                                if (debugDetail) m_log.DebugFormat("{0} prim='{1}': parsed bounding hull. nVerts={2}", LogHeader, primName, mBoundingHull.Count);
                            }

                            if (convexBlock.ContainsKey("HullList")) {
                                byte[] hullList = convexBlock["HullList"].AsBinary();

                                byte[] posBytes = convexBlock["Positions"].AsBinary();

                                List<List<Vector3>> hulls = new List<List<Vector3>>();
                                int posNdx = 0;

                                foreach (byte cnt in hullList) {
                                    int count = cnt == 0 ? 256 : cnt;
                                    List<Vector3> hull = new List<Vector3>();

                                    for (int i = 0; i < count; i++) {
                                        ushort uX = Utils.BytesToUInt16(posBytes, posNdx); posNdx += 2;
                                        ushort uY = Utils.BytesToUInt16(posBytes, posNdx); posNdx += 2;
                                        ushort uZ = Utils.BytesToUInt16(posBytes, posNdx); posNdx += 2;

                                        Vector3 pos = new Vector3(
                                            Utils.UInt16ToFloat(uX, min.X, max.X),
                                            Utils.UInt16ToFloat(uY, min.Y, max.Y),
                                            Utils.UInt16ToFloat(uZ, min.Z, max.Z)
                                        );

                                        hull.Add(pos);
                                    }

                                    hulls.Add(hull);
                                }

                                mConvexHulls = hulls;
                                if (debugDetail) m_log.DebugFormat("{0} prim='{1}': parsed hulls. nHulls={2}", LogHeader, primName, mConvexHulls.Count);
                            }
                            else {
                                if (debugDetail) m_log.DebugFormat("{0} prim='{1}' has physics_convex but no HullList", LogHeader, primName);
                            }
                        }
                    }
                    catch (Exception e) {
                        m_log.WarnFormat("{0} exception decoding convex block: {1}", LogHeader, e);
                    }
                }
                */

                if (meshParams == null) {
                    // m_log.WarnFormat("[MESH]: No recognized physics mesh found in mesh asset for {0}", primName);
                    return null;
                }

                int meshDataOffset = meshParams["offset"].AsInteger() + (int)start;
                int meshDataSize = meshParams["size"].AsInteger();

                if (meshDataOffset < 0 || meshDataSize == 0)
                    return null; // no mesh data in asset

                OSD decodedMeshOsd = new OSD();
                byte[] meshBytes = new byte[meshDataSize];
                System.Buffer.BlockCopy(meshData, meshDataOffset, meshBytes, 0, meshDataSize);
                //                        byte[] decompressed = new byte[physSize * 5];
                try {
                    decodedMeshOsd = DecompressOsd(meshBytes);
                }
                catch (Exception e) {
                    // m_log.ErrorFormat("{0} prim='{1}': exception decoding physical mesh: {2}", LogHeader, primName, e);
                    throw (new Exception("Exception decoding physical mesh " + prim.ID + ": " + e));
                }

                OSDArray decodedMeshOsdArray = null;

                // physics_shape is an array of OSDMaps, one for each submesh
                if (decodedMeshOsd is OSDArray) {
                    //                            Console.WriteLine("decodedMeshOsd for {0} - {1}", primName, Util.GetFormattedXml(decodedMeshOsd));

                    decodedMeshOsdArray = (OSDArray)decodedMeshOsd;
                    foreach (OSD subMeshOsd in decodedMeshOsdArray) {
                        if (subMeshOsd is OSDMap)
                            AddSubMesh(subMeshOsd as OSDMap, size, coords, faces);
                    }
                    // m_log.DebugFormat("{0} {1}: mesh decoded. offset={2}, size={3}, nCoords={4}, nFaces={5}",
                    //             LogHeader, primName, meshDataOffset, meshDataSize, coords.Count, faces.Count);
                }
            }

            OMVR.FacetedMesh omvrmesh = new OMVR.FacetedMesh();


            return omvrmesh;
        }

        /// <summary>
        /// Decodes mesh asset.
        /// <returns>OSDMap of all of the submeshes in the mesh. The value of the submesh name
        /// is the uncompressed data for that mesh.
        /// The OSDMap is made up of the asset_header section (which includes a lot of stuff)
        /// plus each of the submeshes unpacked into compressed byte arrays.
        /// </returns>
        public OSDMap UnpackMesh(byte[] assetData)
        {
            OSDMap meshData = new OSDMap();
            try
            {
                using (MemoryStream data = new MemoryStream(assetData))
                {
                    OSDMap header = (OSDMap)OSDParser.DeserializeLLSDBinary(data);
                    meshData["asset_header"] = header;
                    long start = data.Position;

                    foreach(string partName in header.Keys)
                    {
                        if (header[partName].Type != OSDType.Map)
                        {
                            meshData[partName] = header[partName];
                            continue;
                        }

                        OSDMap partInfo = (OSDMap)header[partName];
                        if (partInfo["offset"] < 0 || partInfo["size"] == 0)
                        {
                            meshData[partName] = partInfo;
                            continue;
                        }

                        byte[] part = new byte[partInfo["size"]];
                        Buffer.BlockCopy(assetData, partInfo["offset"] + (int)start, part, 0, part.Length);
                        meshData[partName] = part;
                        // meshData[partName] = Helpers.ZDecompressOSD(part);   // Do decompression at unpack time
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to decode mesh asset", Helpers.LogLevel.Error, ex);
                meshData = null;
            }
            return meshData;
        }

        // Local routine to create a mesh from prim parameters.
        // Collects parameters and calls PrimMesher to create all the faces of the prim.
        private PrimMesher.PrimMesh GeneratePrimMesh(Primitive prim, DetailLevel lod, bool viewerMode)
        {
            OMV.Primitive.ConstructionData primData = prim.PrimData;
            int sides = 4;
            int hollowsides = 4;

            float profileBegin = primData.ProfileBegin;
            float profileEnd = primData.ProfileEnd;

            bool isSphere = false;

            if ((OMV.ProfileCurve)(primData.profileCurve & 0x07) == OMV.ProfileCurve.Circle)
            {
                switch (lod)
                {
                    case OMVR.DetailLevel.Low:
                        sides = 6;
                        break;
                    case OMVR.DetailLevel.Medium:
                        sides = 12;
                        break;
                    default:
                        sides = 24;
                        break;
                }
            }
            else if ((OMV.ProfileCurve)(primData.profileCurve & 0x07) == OMV.ProfileCurve.EqualTriangle)
                sides = 3;
            else if ((OMV.ProfileCurve)(primData.profileCurve & 0x07) == OMV.ProfileCurve.HalfCircle)
            {
                // half circle, prim is a sphere
                isSphere = true;
                switch (lod)
                {
                    case OMVR.DetailLevel.Low:
                        sides = 6;
                        break;
                    case OMVR.DetailLevel.Medium:
                        sides = 12;
                        break;
                    default:
                        sides = 24;
                        break;
                }
                profileBegin = 0.5f * profileBegin + 0.5f;
                profileEnd = 0.5f * profileEnd + 0.5f;
            }

            if ((OMV.HoleType)primData.ProfileHole == OMV.HoleType.Same)
                hollowsides = sides;
            else if ((OMV.HoleType)primData.ProfileHole == OMV.HoleType.Circle)
            {
                switch (lod)
                {
                    case OMVR.DetailLevel.Low:
                        hollowsides = 6;
                        break;
                    case OMVR.DetailLevel.Medium:
                        hollowsides = 12;
                        break;
                    default:
                        hollowsides = 24;
                        break;
                }
            }
            else if ((OMV.HoleType)primData.ProfileHole == OMV.HoleType.Triangle)
                hollowsides = 3;

            PrimMesher.PrimMesh newPrim = new PrimMesher.PrimMesh(sides, profileBegin, profileEnd, (float)primData.ProfileHollow, hollowsides);
            newPrim.viewerMode = viewerMode;
            newPrim.sphereMode = isSphere;
            newPrim.holeSizeX = primData.PathScaleX;
            newPrim.holeSizeY = primData.PathScaleY;
            newPrim.pathCutBegin = primData.PathBegin;
            newPrim.pathCutEnd = primData.PathEnd;
            newPrim.topShearX = primData.PathShearX;
            newPrim.topShearY = primData.PathShearY;
            newPrim.radius = primData.PathRadiusOffset;
            newPrim.revolutions = primData.PathRevolutions;
            newPrim.skew = primData.PathSkew;
            switch (lod)
            {
                case OMVR.DetailLevel.Low:
                    newPrim.stepsPerRevolution = 6;
                    break;
                case OMVR.DetailLevel.Medium:
                    newPrim.stepsPerRevolution = 12;
                    break;
                default:
                    newPrim.stepsPerRevolution = 24;
                    break;
            }

            if ((primData.PathCurve == OMV.PathCurve.Line) || (primData.PathCurve == OMV.PathCurve.Flexible))
            {
                newPrim.taperX = 1.0f - primData.PathScaleX;
                newPrim.taperY = 1.0f - primData.PathScaleY;
                newPrim.twistBegin = (int)(180 * primData.PathTwistBegin);
                newPrim.twistEnd = (int)(180 * primData.PathTwist);
                newPrim.ExtrudeLinear();
            }
            else
            {
                newPrim.taperX = primData.PathTaperX;
                newPrim.taperY = primData.PathTaperY;
                newPrim.twistBegin = (int)(360 * primData.PathTwistBegin);
                newPrim.twistEnd = (int)(360 * primData.PathTwist);
                newPrim.ExtrudeCircular();
            }

            return newPrim;
        }

        /// <summary>
        /// Method for generating mesh Face from a heightmap
        /// </summary>
        /// <param name="zMap">Two dimension array of floats containing height information</param>
        /// <param name="xBegin">Starting value for X</param>
        /// <param name="xEnd">Max value for X</param>
        /// <param name="yBegin">Starting value for Y</param>
        /// <param name="yEnd">Max value of Y</param>
        /// <returns></returns>
        public OMVR.Face TerrainMesh(float[,] zMap, float xBegin, float xEnd, float yBegin, float yEnd)
        {
            PrimMesher.SculptMesh newMesh = new PrimMesher.SculptMesh(zMap, xBegin, xEnd, yBegin, yEnd, true);
            OMVR.Face terrain = new OMVR.Face();
            int faceVertices = newMesh.coords.Count;
            terrain.Vertices = new List<Vertex>(faceVertices);
            terrain.Indices = new List<ushort>(newMesh.faces.Count * 3);

            for (int j = 0; j < faceVertices; j++)
            {
                var vert = new OMVR.Vertex();
                vert.Position = new Vector3(newMesh.coords[j].X, newMesh.coords[j].Y, newMesh.coords[j].Z);
                vert.Normal = new Vector3(newMesh.normals[j].X, newMesh.normals[j].Y, newMesh.normals[j].Z);
                vert.TexCoord = new Vector2(newMesh.uvs[j].U, newMesh.uvs[j].V);
                terrain.Vertices.Add(vert);
            }

            for (int j = 0; j < newMesh.faces.Count; j++)
            {
                terrain.Indices.Add((ushort)newMesh.faces[j].v1);
                terrain.Indices.Add((ushort)newMesh.faces[j].v2);
                terrain.Indices.Add((ushort)newMesh.faces[j].v3);
            }

            return terrain;
        }
    }
}
