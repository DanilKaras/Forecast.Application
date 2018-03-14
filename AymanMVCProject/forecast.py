import sys
import pandas as pd 
import matplotlib.pyplot as plt
import matplotlib as mpl
import numpy
import matplotlib
from matplotlib import pylab, mlab, pyplot
from IPython.core.pylabtools import figsize, getfigs

from fbprophet import Prophet
from datetime import datetime 
from datetime import timedelta 

np = numpy
plt = pyplot

plt.style.use('ggplot')
pylab.rcParams['figure.figsize'] = (10, 6)

pd.options.display.float_format = '{:,.10f}'.format

df = pd.read_csv('/Users/danylokaras/Downloads/agrs300.csv', parse_dates=['Time'])

df.set_index('Time', inplace=True)
pd.set_option('display.height', None)
pd.set_option('display.max_rows', None)

df['ds'] = df.index
df['y'] = df['avg']

forecast_data = df[['ds', 'y']].copy()
forecast_data.reset_index(inplace=True)
del forecast_data['Time']

m = Prophet()
#m = Prophet(yearly_seasonality = False, weekly_seasonality=True)
#m.add_seasonality(name='monthly', period=30.5, fourier_order=5)
m.add_seasonality(name='hourly', period=0.04, fourier_order=5)
m.add_seasonality(name='daily', period=1, fourier_order=5)
m.fit(forecast_data)

future = m.make_future_dataframe(periods=72, freq='H')
forecast = m.predict(future)
fig = m.plot(forecast, xlabel='Date', ylabel='Price (BTC)')
plt.title('ZRX price forecast 3days (BTC)')
fig.gca().yaxis.set_major_formatter(mpl.ticker.StrMethodFormatter('{x:,.10f}'))

fig.savefig('/Users/danylokaras/fromCSharp.png')
