
// Squre（平方）
inline float sqr(float x)
{
    return x * x;
}

// ふたつのベクトルを合成する
float3 Constraint(float3 potential, float3 normal, float alpha)
{
    // (N・ψ(X))を計算する
    float dp = dot(potential, normal);

    // ψ_constrained(X) = αψ(X) + (1 - α)N(N・ψ(X))
    return (alpha * potential) + ((1.0 - alpha) * dp * normal);
}

// // 計算点から、障害物への距離を計算する
// float SampleDistance(float3 p, float _SphereParam)
// {
//     float3 u = p - _SphereParam.xyz;
//     float d = length(u);
//     return d - _SphereParam.w;
// }

// // α = ramp(d(x)/d0)
// // ψ_constrainted(x) = αψ(x) + (1 - α)n(n・ψ(x))
// float3 SamplePotential(float3 pos, float time)
// {
//     float3 normal = ComputeGradient(pos);
//     float distance = SampleDistance(pos);

//     float3 psi = float3(0, 0, 0);

//     // 高さに応じて乱流の度合いを変化させる（上にいくほど拡散するように）
//     float heightFactor = Ramp((pos.y - _PlumeBase) / _PlumeHeight);
//     for (int i = 0; i < 3; i++)
//     {
//         float alpha = Ramp(abs(distance) / _NoiseScales[i]);

//         float3 s = pos / _NoiseScales[i];

//         float3 psi_i = Constraint(Pnoise(s), normal, alpha);
//         psi += psi_i * heightFactor * _NoiseGain[i];
//     }

//     float3 risingForce = _SphereParam.xyz - pos;
//     risingForce = float3(-risingForce.z, 0, risingForce.x);

//     // ringの半径？
//     // XZ平面の中心からの半径？ RingRadius？
//     float rr = sqrt(pos.x * pos.x + pos.z * pos.z);
//     float temp = sqr(rr - _RingRadius) + sqr(rr + _RingRadius) + _RingFalloff;
//     float invSecond = 1.0 / _RingPerSecond;
//     float ringY = _PlumeCeiling;
//     float alpha = Ramp(abs(distance) / _RingRadius);

//     // 「煙の柱（Plume）」の下端以下になるまで繰り返す
//     while (ringY > _PlumeBase)
//     {
//         // ringの位置とパーティクルのYの差分
//         float ry = pos.y - ringY;

//         float b = temp + sqr(ry);
//         float rmag = _RingMagnitude / b;

//         float3 rpsi = rmag * risingForce;
//         psi += Constraint(rpsi, normal, alpha);
//         ringY -= _RingSpeed * invSecond;
//     }

//     return psi;
// }




// // 勾配（gradient）を計算する
// // 基本的な考えは偏微分が法線ベクトルとなることを利用している？
// float3 ComputeGradient(float3 p)
// {
//     const float e = 0.01f;

//     // 偏微分するため、各軸の微小値を計算する
//     const float3 dx = float3(e, 0, 0);
//     const float3 dy = float3(0, e, 0);
//     const float3 dz = float3(0, 0, e);

//     float d = SampleDistance(p);
//     float dfdx = SampleDistance(p + dx) - d;
//     float dfdy = SampleDistance(p + dy) - d;
//     float dfdz = SampleDistance(p + dz) - d;

//     return normalize(float3(dfdx, dfdy, dfdz));
// }

#include "Packages/jp.keijiro.noiseshader/Shader/ClassicNoise3D.hlsl"
// float Noise(float3 vec)
// {
//     int X = (int)floor(vec.x) & 255;
//     int Y = (int)floor(vec.y) & 255;
//     int Z = (int)floor(vec.z) & 255;

//     vec.x -= floor(vec.x);
//     vec.y -= floor(vec.y);
//     vec.z -= floor(vec.z);

//     float u = Fade(vec.x);
//     float v = Fade(vec.y);
//     float w = Fade(vec.z);

//     int A, AA, AB, B, BA, BB;

//     A = _P[X + 0] + Y; AA = _P[A] + Z; AB = _P[A + 1] + Z;
//     B = _P[X + 1] + Y; BA = _P[B] + Z; BB = _P[B + 1] + Z;

//     return Lerp(w, Lerp(v, Lerp(u, Grad(_P[AA + 0], vec.x + 0, vec.y + 0, vec.z + 0),
//                                     Grad(_P[BA + 0], vec.x - 1, vec.y + 0, vec.z + 0)),
//                             Lerp(u, Grad(_P[AB + 0], vec.x + 0, vec.y - 1, vec.z + 0),
//                                     Grad(_P[BB + 0], vec.x - 1, vec.y - 1, vec.z + 0))),
//                     Lerp(v, Lerp(u, Grad(_P[AA + 1], vec.x + 0, vec.y + 0, vec.z - 1),
//                                     Grad(_P[BA + 1], vec.x - 1, vec.y + 0, vec.z - 1)),
//                             Lerp(u, Grad(_P[AB + 1], vec.x + 0, vec.y - 1, vec.z - 1),
//                                     Grad(_P[BB + 1], vec.x - 1, vec.y - 1, vec.z - 1))));
// }
// float PerlinNoise(float3 vec)
// {
//     float result = 0;
//     float amp = 1.0;

//     result += Noise(vec) * amp;
//     vec *= 2.0;
//     amp *= 0.5;

//     for (int i = 0; i < _Octaves; i++)
//     {
//         result += Noise(vec) * amp;
//         vec *= 2.0;
//         amp *= 0.5;
//     }

//     return result;
// }


// パーリンノイズによるベクトル場
// 3Dとして3要素を計算。
// それぞれのノイズは明らかに違う（極端に大きなオフセット）を持たせた値とする
float3 Pnoise(float3 vec, float frequency = 0.3)
{
    // float x = PerlinNoise(vec);
    float x = ClassicNoise(vec*frequency);

    // float y = PerlinNoise(float3(
    float y = ClassicNoise(float3(
        vec.y + 31.416,
        vec.z - 47.853,
        vec.x + 12.793
    )*frequency);

    // float z = PerlinNoise(float3(
    float z = ClassicNoise(float3(
        vec.z - 233.145,
        vec.x - 113.408,
        vec.y - 185.31
    )*frequency);

    return float3(x, y, z);
}
float3 SamplePotential(float3 pos, float time)
{
    // float3 s = pos / _NoiseScales[0];
    float3 s = pos / 1;
    return Pnoise(s);
}


// float3 CurlNoise(Particle p)
float3 CurlNoise(float3 position, float time)
{
    //const float e = 0.0009765625;
    const float e = 1e-4f;
    const float e2 = 2.0 * e;
    const float invE2 = 1.0 / e2;

    const float3 dx = float3(e, 0.0, 0.0);
    const float3 dy = float3(0.0, e, 0.0);
    const float3 dz = float3(0.0, 0.0, e);

    float3 pos = position;

    float3 p_x0 = SamplePotential(pos - dx, time);
    float3 p_x1 = SamplePotential(pos + dx, time);
    float3 p_y0 = SamplePotential(pos - dy, time);
    float3 p_y1 = SamplePotential(pos + dy, time);
    float3 p_z0 = SamplePotential(pos - dz, time);
    float3 p_z1 = SamplePotential(pos + dz, time);

    float x = (p_y1.z - p_y0.z) - (p_z1.y - p_z0.y);
    float y = (p_z1.x - p_z0.x) - (p_x1.z - p_x0.z);
    float z = (p_x1.y - p_x0.y) - (p_y1.x - p_y0.x);

    return float3(x, y, z) * invE2;
}