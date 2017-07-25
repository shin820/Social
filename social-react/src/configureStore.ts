import { createStore, applyMiddleware, compose, combineReducers, Store } from 'redux';
import thunk from 'redux-thunk';
// import thunk from 'redux-thunk';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import { ApplicationState, reducers } from './store';
import { History } from 'history';


export default function configureStore(history: History, initialState?: ApplicationState) {

    // // Build middleware. These are functions that can process the actions before they reach the store.
    // const windowIfDefined = typeof window === 'undefined' ? null : window as any;
    // // If devTools is installed, connect to it
    // const devToolsExtension = windowIfDefined && windowIfDefined.devToolsExtension as () => GenericStoreEnhancer;
    const createStoreWithMiddleware = compose(
        applyMiddleware(thunk, routerMiddleware(history))
    )(createStore);

    const allReducers = buildRootReducer(reducers);
    const store = createStoreWithMiddleware(allReducers, initialState) as Store<ApplicationState>;

    return store;
}

function buildRootReducer(allReducers) {
    return combineReducers<ApplicationState>(Object.assign({}, allReducers, { routing: routerReducer }));
}
