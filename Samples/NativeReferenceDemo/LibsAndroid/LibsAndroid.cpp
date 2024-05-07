#include "LibsAndroid.h"

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "LibsAndroid", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "LibsAndroid", __VA_ARGS__))

extern "C" {
	/*此简单函数返回平台 ABI，此动态本地库为此平台 ABI 进行编译。*/
	const char * LibsAndroid::getPlatformABI()
	{
	#if defined(__arm__)
	#if defined(__ARM_ARCH_7A__)
	#if defined(__ARM_NEON__)
		#define ABI "armeabi-v7a/NEON"
	#else
		#define ABI "armeabi-v7a"
	#endif
	#else
		#define ABI "armeabi"
	#endif
	#elif defined(__i386__)
		#define ABI "x86"
	#else
		#define ABI "unknown"
	#endif
		LOGI("This dynamic shared library is compiled with ABI: %s", ABI);
		return "This native library is compiled with ABI: %s" ABI ".";
	}

	void LibsAndroid()
	{
	}

    int clickCount = 666;
    int getClickCount() {
        return clickCount++;
    } 

	LibsAndroid::LibsAndroid()
	{
	}

	LibsAndroid::~LibsAndroid()
	{
	}
}

extern "C" int getClickCount2(){
    return clickCount++;
}