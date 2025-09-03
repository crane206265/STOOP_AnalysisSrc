import numpy as np
import matplotlib.pyplot as plt

def phase(alpha):
    alpha = np.where(alpha < np.pi, alpha, 2*np.pi-alpha)
    return np.sin(alpha) - alpha*np.cos(alpha)

x = np.linspace(0, 2*np.pi, 201)
plt.plot(x, phase(x))
plt.plot(x, np.zeros_like(x), linestyle='dashed')
plt.plot(x, np.pi*np.ones_like(x), linestyle='dashed')
plt.show()