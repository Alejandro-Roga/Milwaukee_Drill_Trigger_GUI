#ifndef BREITHORNDRIVERWRAPPER_H
#define BREITHORNDRIVERWRAPPER_H

#include "breithorn-driver-wrapper_global.h"

extern "C"{

/**
 * @brief init
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int init(void);

/**
 * @brief close
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int close(void);

/**
 * @brief setMaSiliconVersion
 * @param version
 *    0 = auto detection (not tested)
 *    1 = MA100, MA300, MA700, MA750
 *    2 = MA300A, MA700A
 *    3 = MA102, MA302, MA310, MA702, MA710, MA730, MA800, MA820, MA850, MA732, MA731, MA704
 *    4 = MA780, MA781
 * @return error
 *  error code:
 *      0 = no error, EVKT-MACOM is connected
 *     -1 = EVKT-MACOM not connected to the computer, check USB connection
 *     -2 = MagAlpha version number not supported
 *     -3 = MagAlpha sensor not connected to the EVKT-MACOM, check the sensor connection
 *     -4 = Auto detection failed to recognize the connected sensor
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int setMaSiliconVersion(unsigned char version);

/**
 * @brief readMaRegister
 * @param address
 * @return register value
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT unsigned char readMaRegister(unsigned char address);

/**
 * @brief writeMaRegister
 * @param address
 * @param value
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int writeMaRegister(unsigned char address, unsigned char value);

/**
 * @brief readMagAlphaAngularPosition
 *     This API trigger an acquisition of the sensor data (other APIs below only return value computed from this acquisition)
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int readMagAlphaAngularPosition(void);

/**
 * @brief getMaAngularPositionArray
 * @param angleArrayRaw
 * @param angleArrayDegree
 * @param arraySize
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int getMaAngularPositionArray(unsigned short *angleArrayRaw, double *angleArrayDegree, int arraySize);

/**
 * @brief getMaSignalParameters
 * @param meanAngle
 * @param medianAngle
 * @param resolutionInBit
 * @param resolutionInDegree
 * @param standardDeviation
 * @param variance
 * @param period
 * @param speed
 * @param inl
 * @param fitQuality
 * @param h1
 * @param phi1
 * @param h2
 * @param phi2
 * @param h3
 * @param phi3
 * @param h4
 * @param phi4
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT void getMaSignalParameters( double *meanAngle,
                                                                double *medianAngle,
                                                                double *resolutionInBit,
                                                                double *resolutionInDegree,
                                                                double *standardDeviation,
                                                                double *variance,
                                                                double *period,
                                                                double *speed,
                                                                double *inl,
                                                                double *fitQuality,
                                                                double *h1,
                                                                double *phi1,
                                                                double *h2,
                                                                double *phi2,
                                                                double *h3,
                                                                double *phi3,
                                                                double *h4,
                                                                double *phi4);

/**
 * @brief getMaMeanAngularPosition
 * @return mean angular position
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT double getMaMeanAngularPosition(void);

/**
 * @brief getMaResolution
 * @return resoultion in bits
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT double getMaResolution(void);

/**
 * @brief getMaStandardDeviation
 * @return standard deviation
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT double getMaStandardDeviation(void);

/**
 * @brief setMaSamplingRate
 * @param samplingRateInUs
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int setMaSamplingRate(unsigned int samplingRateInUs);

/**
 * @brief setMaSpiClockFreq
 * @param spiClkFreqInkHz
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int setMaSpiClockFreq(unsigned int spiClkFreqInkHz);

/**
 * @brief setMaNumberOfSampleToAcquire
 * @param numberOfSample
 * @return error (0 = no error)
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT int setMaNumberOfSampleToAcquire(unsigned int numberOfSample);

/**
 * @brief getMaNumberOfSampleToAcquire
 * @return number of sample
 */
BREITHORNDRIVERWRAPPERSHARED_EXPORT unsigned int getMaNumberOfSampleToAcquire(void);

}//extern "C"
#endif // BREITHORNDRIVERWRAPPER_H
