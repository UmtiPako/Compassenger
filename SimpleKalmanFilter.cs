public class SimpleKalmanFilter
{
    private double _state; // Tahmin edilen mevcut durum
    private double _kalmanGain; // Kalman kazancı

    // Gürültü parametreleri
    private const double ProcessNoise = 0.001; // Süreç gürültüsü: Sistemin ne kadar kararsız olduğunu belirtir.
    private const double MeasurementNoise = 0.5; // Ölçüm gürültüsü: Sensörün ne kadar gürültülü olduğunu belirtir.

    public SimpleKalmanFilter(double initialValue)
    {
        _state = initialValue;
    }

    public double Filter(double measurement)
    {
        // 1. Tahmin Aşaması:
        // Durumu bir önceki durum olarak tahmin et
        // Gürültüyü ekle
        double predictedState = _state + ProcessNoise;

        // 2. Düzeltme Aşaması:
        // Kalman kazancını hesapla
        _kalmanGain = predictedState / (predictedState + MeasurementNoise);

        // Durumu yeni ölçümle düzelt
        _state = _state + _kalmanGain * (measurement - _state);

        return _state;
    }
}