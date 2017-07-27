import * as React from 'react';
import { connect, Dispatch } from 'react-redux';
import { FilterListItem, actionCreators, KnownAction } from '../../store/FilterList';

// At runtime, Redux will merge together...
// type FilterListProps =
//     FilterListState.FilterListState
//     & typeof FilterListState.actionCreators;
export interface FilterListProps {
    items: FilterListItem[]
    requestFilterList: () => void;
}

function FilterList({ items, requestFilterList }: FilterListProps) {
    items = items || new Array<FilterListItem>();
    return <div>
        <div>Filters</div>
        <div>
            <ul>
                {items.map((filter,index) => <li key={index}>{filter.name} - {filter.unReadNum}</li>)}
            </ul>
            <button onClick={requestFilterList}>reload</button>
        </div>
    </div>
}

export function mapStateToProps(state: FilterListItem[]) {
    return {
        items: state
    }
}

export function mapDispatchToProps(dispatch: Dispatch<KnownAction>) {
    return {
        requestFilterList: () => dispatch(actionCreators.requestFilterList())
    }
}

export default connect(
    mapStateToProps, // Selects which state properties are merged into the component's props
    mapDispatchToProps               // Selects which action creators are merged into the component's props
)(FilterList);