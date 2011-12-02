################################
#
# QMake definitions for library
#

include ($$(ARTKP)/build/linux/options.pro)

TEMPLATE = lib

TARGET   = ARToolKitPlus

QMAKE_CLEAN = $$(ARTKP)/lib/*

DESTDIR  = $$(ARTKP)/lib

debug {
  message("Building ARToolKitPlus in debug mode ...")
}

release {
  message("Building ARToolKitPlus in release mode ...")
}

SOURCES = MemoryManager.cpp \
        DLL.cpp \
	librpp/rpp.cpp \
	librpp/rpp_quintic.cpp \
	librpp/rpp_vecmat.cpp \
	librpp/rpp_svd.cpp \
	librpp/librpp.cpp \
        extra/Profiler.cpp \
        extra/FixedPoint.cpp

HEADERS = \
        ../include/ARToolKitPlus/ARToolKitPlus.h \
        ../include/ARToolKitPlus/Camera.h \
        ../include/ARToolKitPlus/CameraAdvImpl.h \
        ../include/ARToolKitPlus/CameraFactory.h \
        ../include/ARToolKitPlus/CameraImpl.h \
        ../include/ARToolKitPlus/ImageGrabber.h \
        ../include/ARToolKitPlus/Logger.h \
        ../include/ARToolKitPlus/MemoryManager.h \
        ../include/ARToolKitPlus/MemoryManagerMemMap.h \
        ../include/ARToolKitPlus/Tracker.h \
        ../include/ARToolKitPlus/TrackerImpl.h \
        ../include/ARToolKitPlus/TrackerMultiMarker.h \
        ../include/ARToolKitPlus/TrackerMultiMarkerImpl.h \
        ../include/ARToolKitPlus/TrackerSingleMarker.h \
        ../include/ARToolKitPlus/TrackerSingleMarkerImpl.h \
        ../include/ARToolKitPlus/ar.h \
        ../include/ARToolKitPlus/arBitFieldPattern.h \
        ../include/ARToolKitPlus/arMulti.h \
        ../include/ARToolKitPlus/byteSwap.h \
        ../include/ARToolKitPlus/config.h \
        ../include/ARToolKitPlus/matrix.h \
        ../include/ARToolKitPlus/param.h \
        ../include/ARToolKitPlus/vector.h

HEADERS_EXTRA = \
        ../include/ARToolKitPlus/extra/BCH.h \
        ../include/ARToolKitPlus/extra/GPP.h \
        ../include/ARToolKitPlus/extra/Profiler.h \
        ../include/ARToolKitPlus/extra/rpp.h

target.path = ""/$$LIBDIR
headers.path = ""/$$PREFIX/include/ARToolKitPlus
headers.files = $$HEADERS
headers_extra.path = ""/$$PREFIX/include/ARToolKitPlus/extra
headers_extra.files = $$HEADERS_EXTRA
doc.path = ""/$$PREFIX/share/doc/packages/ARToolKitPlus
doc.files = ../doc/ART02-Tutorial.pdf
data_simple.path = ""/$$PREFIX/share/ARToolKitPlus/simple/data
data_simple.files = ../sample/simple/data/*.dat ../sample/simple/data/*.cal ../sample/simple/data/*.raw ../sample/simple/data/*.jpg
data_multi.path = ""/$$PREFIX/share/ARToolKitPlus/multi/data
data_multi.files = ../sample/multi/data/*.dat ../sample/multi/data/*.cal ../sample/multi/data/*.raw ../sample/multi/data/*.jpg

INSTALLS += target headers headers_extra doc data_simple data_multi

################################
